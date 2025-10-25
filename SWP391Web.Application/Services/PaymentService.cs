using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Payment;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SWP391Web.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly string _baseUrl, _tmnCode, _hashSecret, _returnUrl, _ipnUrl;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _http;
        public PaymentService(IConfiguration cfg, IUnitOfWork unitOfWork, IHttpContextAccessor httpContext)
        {
            _baseUrl = cfg["VNPay:BaseUrl"] ?? throw new Exception("Cannot find VNPay:BaseUrl");
            _tmnCode = cfg["VNPay:TmnCode"] ?? throw new Exception("Cannot find VNPay:TmnCode");
            _hashSecret = cfg["VNPay:HashSecret"] ?? throw new Exception("Cannot find VNPay:HashSecret");
            _returnUrl = cfg["VNPay:ReturnUrl"] ?? throw new Exception("Cannot find VNPay:ReturnUrl");
            //_ipnUrl = cfg["VNPay:IpnUrl"] ?? throw new Exception("Cannot find VNPay:IpnUrl");
            _unitOfWork = unitOfWork;
            _http = httpContext;
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
                if (order.Status.Equals(OrderStatus.Confirmed) && order.DepositAmount is not null)
                {
                    amount = order.DepositAmount;
                }
                else if (order.Status.Equals(OrderStatus.Deposited) && order.DepositAmount is not null)
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

        private static bool IsPrivate(System.Net.IPAddress ip)
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

            if (!System.Net.IPAddress.TryParse(candidate, out var ip))
                return "127.0.0.1";

            ip = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? ip.MapToIPv4() : ip;
            var ipv4 = ip.ToString();

            if (IsPrivate(ip))
            {
                var force = Environment.GetEnvironmentVariable("VNPAY_FORCE_CLIENT_IP");
                if (!string.IsNullOrWhiteSpace(force) && System.Net.IPAddress.TryParse(force, out var forced))
                    return forced.MapToIPv4().ToString();
            }

            return ipv4;
        }

        public async Task<ResponseDTO> HandleVNPayIpn(VNPayIPNDTO ipnDTO)
        {
            try
            {
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

                string FormEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
                var signData = string.Join("&", data.Select(kvp => $"{kvp.Key}={FormEncode(kvp.Value)}"));
                var result = HmacSha512(_hashSecret, signData);

                if (!string.Equals(result, ipnDTO.Vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
                {
                    return new ResponseDTO()
                    {
                        StatusCode = 200,
                        Message = "Invalid checksum",
                        Result = new { RspCode = "97", Message = "Invalid signature" }
                    };
                }

                if (ipnDTO.Vnp_ResponseCode == "00" && ipnDTO.Vnp_TransactionStatus == "00")
                {
                    var order = await _unitOfWork.CustomerOrderRepository.GetByOrderNoAsync(int.Parse(ipnDTO.Vnp_TxnRef));
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

                    await HandleVNPayCustomerOrder(order, decimal.Parse(ipnDTO.Vnp_Amount));
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

        private async Task HandleVNPayCustomerOrder(CustomerOrder customerOrder, decimal amount)
        {
            if (amount == customerOrder.TotalAmount || amount == (customerOrder.TotalAmount - customerOrder.DepositAmount))
            {
                await HandleVehicleInOrder(customerOrder);
                customerOrder.Status = OrderStatus.Completed;
            }
            else
            {
                customerOrder.Status = OrderStatus.Deposited;
            }

            await _unitOfWork.SaveAsync();
        }

        private async Task HandleVehicleInOrder(CustomerOrder customerOrder)
        {
            foreach (var detail in customerOrder.OrderDetails)
            {
                var ev = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(detail.ElectricVehicleId);
                if (ev is null)
                {
                    throw new Exception($"Cannot find the electric vehicle in orderNo {customerOrder.OrderNo}");
                }
                ev.Status = ElectricVehicleStatus.Booked;
            }
        }
    }
}
