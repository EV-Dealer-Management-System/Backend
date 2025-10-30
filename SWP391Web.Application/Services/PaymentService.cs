using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Payment;
using SWP391Web.Application.IService;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Constants;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using SWP391Web.Infrastructure.SignlR;
using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SWP391Web.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly string _baseUrl, _tmnCode, _hashSecret, _returnUrl;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _http;
        private readonly IEmailService _emailService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public PaymentService(IConfiguration cfg, IUnitOfWork unitOfWork, IHttpContextAccessor httpContext, IEmailService emailService, IHubContext<NotificationHub> hubContext)
        {
            _baseUrl = cfg["VNPay:BaseUrl"] ?? throw new Exception("Cannot find VNPay:BaseUrl");
            _tmnCode = cfg["VNPay:TmnCode"] ?? throw new Exception("Cannot find VNPay:TmnCode");
            _hashSecret = cfg["VNPay:HashSecret"] ?? throw new Exception("Cannot find VNPay:HashSecret");
            _returnUrl = cfg["VNPay:ReturnUrl"] ?? throw new Exception("Cannot find VNPay:ReturnUrl");
            _unitOfWork = unitOfWork;
            _http = httpContext;
            _emailService = emailService;
            _hubContext = hubContext;
        }
        public async Task<ResponseDTO> CreateVNPayLink(Guid customerOrderId, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);

                if (order == null)
                {
                    return new ResponseDTO(false)
                    {
                        Message = "Order not found",
                        StatusCode = 404
                    };
                }

                decimal? amount;
                if (order.Status.Equals(OrderStatus.FullPending) && order.DepositAmount is not null)
                {
                    amount = order.DepositAmount;
                }
                else if (order.Status.Equals(OrderStatus.DepositPending) && order.DepositAmount is not null)
                {
                    amount = (order.TotalAmount - order.DepositAmount);
                }
                else
                {
                    amount = order.TotalAmount;
                }

                amount = (amount * 100);
                var createDate = ToGmt7(DateTime.UtcNow);
                var OrderInfo = $"CodeNo-{order.OrderNo}-Price-{amount}";
                var expireDate = ToGmt7(DateTime.UtcNow.AddMinutes(15));
                var orderNo = order.OrderNo.ToString();
                var clientIp = ResolveClientIp();

                var data = new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    ["vnp_Version"] = "2.1.0",
                    ["vnp_Command"] = "pay",
                    ["vnp_TmnCode"] = _tmnCode,
                    ["vnp_Amount"] = amount.ToString()!,
                    ["vnp_CreateDate"] = createDate,
                    ["vnp_CurrCode"] = "VND",
                    ["vnp_IpAddr"] = clientIp,
                    ["vnp_Locale"] = "vn",
                    ["vnp_OrderInfo"] = OrderInfo,
                    ["vnp_OrderType"] = "240000",
                    ["vnp_ReturnUrl"] = _returnUrl,
                    ["vnp_ExpireDate"] = expireDate,
                    ["vnp_TxnRef"] = orderNo
                };

                string FormEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
                var signData = string.Join("&", data.Select(kvp => $"{kvp.Key}={FormEncode(kvp.Value)}"));
                var secureHash = HmacSha512(_hashSecret, signData);

                var queryString = signData + $"&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
                var paymentUrl = _baseUrl + "?" + queryString;

                await _emailService.NotifyPaymentLinkToCustomer(order.Customer.Email!, order.Customer.FullName!, order.OrderNo, amount.Value / 100, paymentUrl);
                return new ResponseDTO()
                {
                    Message = "VNPay link created successfully",
                    Result = paymentUrl,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to create a vnpay link: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private string HmacSha512(string secret, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private string ToGmt7(DateTime utc)
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timezone).ToString("yyyyMMddHHmmss");
            }
            catch
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, timezone).ToString("yyyyMMddHHmmss");
            }
        }

        private bool IsPrivate(IPAddress ip)
        {
            var s = ip.MapToIPv4().ToString();
            if (s.StartsWith("10.") || s.StartsWith("192.168.")) return true;
            if (s.StartsWith("172."))
            {
                var parts = s.Split('.');
                if (parts.Length > 1 && int.TryParse(parts[1], out var b) && b >= 16 && b <= 31) return true;
            }
            return s == "127.0.0.1" || s == "0.0.0.0";
        }

        private string ResolveClientIp()
        {
            var http = _http.HttpContext;
            if (http == null) return "127.0.0.1";

            string? pickFirst(string? csv)
                => csv?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            var candidate =
                   pickFirst(http.Request.Headers["X-Forwarded-For"].FirstOrDefault())
                ?? http.Request.Headers["X-Real-IP"].FirstOrDefault()
                ?? http.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                ?? http.Connection.RemoteIpAddress?.ToString();

            if (!IPAddress.TryParse(candidate, out var ip))
                return "127.0.0.1";

            ip = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? ip.MapToIPv4() : ip;
            var ipv4 = ip.ToString();

            if (IsPrivate(ip))
            {
                var force = Environment.GetEnvironmentVariable("VNPAY_FORCE_CLIENT_IP");
                if (!string.IsNullOrWhiteSpace(force) && IPAddress.TryParse(force, out var forced))
                    return forced.MapToIPv4().ToString();
            }

            return ipv4;
        }

        public async Task<ResponseDTO> HandleVNPayIpn(VNPayIPNDTO ipnDTO, CancellationToken ct)
        {
            try
            {
                Console.WriteLine($"[IPN] 1");
                var data = new SortedDictionary<string, string>(StringComparer.Ordinal);
                foreach (var prop in typeof(VNPayIPNDTO).GetProperties())
                {
                    var key = prop.Name;
                    var value = prop.GetValue(ipnDTO)?.ToString();
                    if (!string.IsNullOrEmpty(value) && key != "vnp_SecureHash")
                    {
                        data[key] = value;
                    }
                }
                Console.WriteLine($"[IPN] 2");

                string FormEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
                var signData = string.Join("&", data.Select(kvp => $"{kvp.Key}={FormEncode(kvp.Value)}"));
                var result = HmacSha512(_hashSecret, signData);

                if (!string.Equals(result, ipnDTO.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseDTO()
                    {
                        StatusCode = 200,
                        Message = "Invalid checksum",
                        Result = new { RspCode = "97", Message = "Invalid signature" }
                    };
                }

                if (ipnDTO.vnp_ResponseCode == "00" && ipnDTO.vnp_TransactionStatus == "00")
                {
                    Console.WriteLine($"[IPN] 3");
                    var order = await _unitOfWork.CustomerOrderRepository.GetByOrderNoAsync(int.Parse(ipnDTO.vnp_TxnRef));
                    if (order is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Order not found.",
                            StatusCode = 404,
                            Result = new
                            {
                                RspCode = "01",
                                Message = "Order not found"
                            }
                        };
                    }

                    Console.WriteLine($"[IPN] 4");
                    var paidAmount = decimal.Parse(ipnDTO.vnp_Amount) / 100;
                    await HandleVNPayCustomerOrder(order, paidAmount, ct);

                    //DateTime dateTimeLocal = DateTime.ParseExact(ipnDTO.vnp_PayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    //DateTime dateTimeUtc = DateTime.SpecifyKind(dateTimeLocal, DateTimeKind.Unspecified).ToUniversalTime();

                    var Transaction = new Transaction
                    {
                        CustomerOrderId = order.Id,
                        Amount = paidAmount,
                        Provider = "VNPay",
                        OrderRef = ipnDTO.vnp_TxnRef,
                        Currency = "VND",
                        Status = TransactionStatus.Success,
                        CreatedAt = DateTime.UtcNow
                    };
                    Console.WriteLine($"[IPN] 13");

                    await _unitOfWork.TransactionRepository.AddAsync(Transaction, ct);
                    Console.WriteLine($"[IPN] 14");

                    return new ResponseDTO()
                    {
                        StatusCode = 200,
                        Message = "Payment successful",
                        Result = new
                        {
                            RspCode = "00",
                            Message = "Confirm success"
                        }
                    };
                }
                Console.WriteLine($"[IPN] 15");
                await _unitOfWork.SaveAsync();
                Console.WriteLine($"[IPN] 16");
                return new ResponseDTO()
                {
                    StatusCode = 200,
                    Message = "Payment failed or canceled",
                    Result = new
                    {
                        RspCode = "02",
                        Message = "Confirm Success, payment failed"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to handle VNPay IPN: {ex.Message}",
                    StatusCode = 500,
                    Result = new
                    {
                        RspCode = "99",
                        Message = "Unknow error"
                    }
                };
            }
        }

        private async Task HandleVNPayCustomerOrder(CustomerOrder customerOrder, decimal amount, CancellationToken ct)
        {
            Console.WriteLine($"[IPN] 6");
            if (amount == customerOrder.TotalAmount || (customerOrder.DepositAmount != null && amount == (customerOrder.TotalAmount - customerOrder.DepositAmount)))
            {
                await HandleVehicleInOrder(customerOrder, ct);
                Console.WriteLine($"[IPN] 11");
                customerOrder.Status = OrderStatus.Completed;
            }
            else
            {
                customerOrder.Status = OrderStatus.Depositing;
            }
            _unitOfWork.CustomerOrderRepository.Update(customerOrder);
            Console.WriteLine($"[IPN] 12");
        }

        private async Task HandleVehicleInOrder(CustomerOrder customerOrder, CancellationToken ct)
        {
            Console.WriteLine($"[IPN] 7");
            var outOfStock = new List<(string modelName, string versionName, string colorName, int quantity)>();
            var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(customerOrder.Quote.DealerId);
            if (warehouse is null)
            {
                throw new Exception($"Cannot find warehouse for dealerId {customerOrder.Quote.DealerId}");
            }
            foreach (var detail in customerOrder.OrderDetails)
            {
                Console.WriteLine($"[IPN] 8");
                var ev = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(detail.ElectricVehicleId);
                if (ev is null)
                {
                    throw new Exception($"Cannot find the electric vehicle in orderNo {customerOrder.OrderNo}");
                }
                ev.Status = ElectricVehicleStatus.Booked;
                _unitOfWork.ElectricVehicleRepository.Update(ev);
                await _unitOfWork.SaveAsync();
                var quantityCurrent = await _unitOfWork.ElectricVehicleRepository.CountDealerAvailableByVersionColorAsync(customerOrder.Quote.DealerId, ev.ElectricVehicleTemplate.VersionId,
                    ev.ElectricVehicleTemplate.ColorId, ct);

                var template = ev.ElectricVehicleTemplate;
                var version = template.Version;
                var model = version.Model;
                var color = template.Color;

                Console.WriteLine($"[IPN] 9");

                if (quantityCurrent <= warehouse.AlertNumber && !outOfStock.Any(o => o.modelName == model.ModelName && o.versionName == version.VersionName &&
                    o.colorName == color.ColorName))
                {
                    Console.WriteLine($"[IPN] 10");
                    outOfStock.Add((modelName: model.ModelName ?? string.Empty,
                        versionName: version.VersionName ?? string.Empty,
                        colorName: color.ColorName ?? string.Empty,
                        quantity: quantityCurrent));
                }
            }

            if (outOfStock.Count > 0)
            {
                await CreateAggregationOutOfStockAsync(customerOrder.Quote.DealerId, outOfStock, ct);
            }
        }

        private async Task CreateAggregationOutOfStockAsync(Guid dealerId, List<(string modelName, string versionName, string colorName, int quantity)> outOfStock, CancellationToken ct)
        {
            if (outOfStock == null || outOfStock.Count == 0)
            {
                return;
            }

            var items = String.Join(", ", outOfStock.Select(i => $"{i.modelName} - {i.versionName} - {i.colorName} còn {i.quantity} xe"));

            var title = "Cảnh báo số lượng xe";
            var message = $"Lưu ý: {items}";

            var notification = new Notification
            {
                DealerId = dealerId,
                Title = title,
                Message = message,
                TargetRole = StaticUserRole.DealerManager,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _unitOfWork.NotificationRepository.AddAsync(notification, ct);
            await _unitOfWork.SaveAsync();

            await _hubContext.Clients.Group($"Dealer_{dealerId}_{StaticUserRole.DealerManager}").SendAsync("NotificationChanged");
        }
    }
}
