using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.DTO.DealerDebt;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IPaymentService _paymentService;
        private readonly IDepositSettingService _depositSetting;
        private readonly IDealerDebtService _dealerDebtService;
        private readonly IDealerDebtTransactionService _dealerDebtTransactionService;
        private readonly IEContractService _eContractService;
        public CustomerOrderService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService, IDepositSettingService depositSetting,
            IDealerDebtService dealerDebtService, IDealerDebtTransactionService dealerDebtTransactionService, IEContractService eContractService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _paymentService = paymentService;
            _depositSetting = depositSetting;
            _dealerDebtService = dealerDebtService;
            _dealerDebtTransactionService = dealerDebtTransactionService;
            _eContractService = eContractService;
        }
        public async Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateCustomerOrderDTO createCustomerOrderDTO, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, CancellationToken.None);
                if (dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found.",
                        StatusCode = 404,
                    };
                }

                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(createCustomerOrderDTO.QuoteId);
                if (quote == null || quote.DealerId != dealer.Id)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found.",
                        StatusCode = 404,
                    };
                }

                if (quote.Status != QuoteStatus.Accepted)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote is not accepted yet. Cann't create order.",
                        StatusCode = 400,
                    };
                }

                var orderNo = _unitOfWork.CustomerOrderRepository.GenerateOrderNumber();

                OrderStatus status;
                var amount = quote.TotalAmount;
                decimal? deposit = null;
                if (createCustomerOrderDTO.IsPayFull && !createCustomerOrderDTO.IsCash)
                {
                    status = OrderStatus.FullPending;
                }
                else if (createCustomerOrderDTO.IsPayFull && createCustomerOrderDTO.IsCash)
                {
                    status = OrderStatus.Completed;

                    var recordPayment = new RecordPaymentDTO
                    {
                        PaidAtUtc = DateTime.UtcNow,
                        Amount = amount,
                        ReferenceNo = $"CustomerOrderId|{orderNo}",
                        Note = $"Full payment for order {orderNo}",
                        Method = "Cash",
                    };

                    await _dealerDebtService.AddPaymentForDealerAsync(dealer.Id, recordPayment, ct);
                }
                else if (!createCustomerOrderDTO.IsPayFull && createCustomerOrderDTO.IsCash)
                {
                    status = OrderStatus.Depositing;
                    var depositRate = await _depositSetting.GetDepositSetting(user, ct);
                    deposit = amount * (depositRate.Data!.MaxDepositPercentage / 100);

                    var recordPayment = new RecordPaymentDTO
                    {
                        PaidAtUtc = DateTime.UtcNow,
                        Amount = deposit.Value,
                        ReferenceNo = $"CustomerOrderId|{orderNo}",
                        Note = $"Deposit payment for order {orderNo}",
                        Method = "Cash",
                    };
                    await _dealerDebtService.AddPaymentForDealerAsync(dealer.Id, recordPayment, ct);
                }
                else
                {
                    status = OrderStatus.DepositPending;
                    var depositRate = await _depositSetting.GetDepositSetting(user, ct);
                    deposit = amount * (depositRate.Data!.MaxDepositPercentage / 100);
                }

                var customerOrder = new CustomerOrder
                {
                    CustomerId = createCustomerOrderDTO.CustomerId,
                    QuoteId = quote.Id,
                    OrderNo = orderNo,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = amount,
                    DepositAmount = deposit.HasValue ? (int)deposit.Value : (int?)null,
                    Status = status,
                    CreatedBy = userId,
                    Quote = quote
                };

                await _unitOfWork.CustomerOrderRepository.AddAsync(customerOrder, ct);

                await HandleOrderDetail(customerOrder, ct);

                var getCustomerOrder = _mapper.Map<GetCustomerOrderDTO>(customerOrder);

                if (!createCustomerOrderDTO.IsCash)
                {
                    await _paymentService.CreateVNPayLink(customerOrder.Id, ct);
                }
                else if (createCustomerOrderDTO.IsCash && !createCustomerOrderDTO.IsPayFull)
                {
                    var transaction = new Transaction
                    {
                        Amount = createCustomerOrderDTO.IsPayFull ? amount : deposit.Value,
                        CustomerOrderId = customerOrder.Id,
                        Status = TransactionStatus.Success,
                        OrderRef = customerOrder.OrderNo.ToString(),
                        Currency = "VND",
                        Note = createCustomerOrderDTO.IsPayFull ? $"Full payment for order {customerOrder.OrderNo}" : $"Deposit payment for order {customerOrder.OrderNo}",
                        Provider = "Cash",
                    };
                    await _unitOfWork.TransactionRepository.AddAsync(transaction, ct);

                    if (!createCustomerOrderDTO.IsPayFull)
                    {
                        await _eContractService.CreateDepositEContractConfirm(customerOrder.Id, ct);
                    }
                }
                else
                {
                    var transaction = new Transaction
                    {
                        Amount = amount,
                        CustomerOrderId = customerOrder.Id,
                        Status = TransactionStatus.Success,
                        OrderRef = customerOrder.OrderNo.ToString(),
                        Currency = "VND",
                        Note = $"Full payment for order {customerOrder.OrderNo}",
                        Provider = "Cash",
                    };

                    await _unitOfWork.TransactionRepository.AddAsync(transaction, ct);

                    await _eContractService.CreatePayFullConfirmationEContract(customerOrder.Id, ct);
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create customer order successfully.",
                    StatusCode = 201,
                    Result = getCustomerOrder,
                };

            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }

        private async Task HandleOrderDetail(CustomerOrder customerOrder, CancellationToken ct)
        {
            var orderDetails = customerOrder.Quote.QuoteDetails;
            foreach (var quoteDetail in orderDetails)
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetVehicleByQuantityWithOldestImportDateForDealerAsync(
                        quoteDetail.VersionId,
                        quoteDetail.ColorId,
                        customerOrder.Quote.Dealer.Warehouse.Id,
                        quoteDetail.Quantity);

                foreach (var vehicle in vehicles)
                {
                    var orderDetail = new OrderDetail
                    {
                        CustomerOrderId = customerOrder.Id,
                        ElectricVehicleId = vehicle.Id
                    };
                    await _unitOfWork.OrderDetailRepository.AddAsync(orderDetail, ct);
                    if (customerOrder.Status is OrderStatus.Completed)
                    {
                        vehicle.Status = ElectricVehicleStatus.Sold;
                    }
                    else if (customerOrder.Status is OrderStatus.Depositing)
                    {
                        vehicle.Status = ElectricVehicleStatus.DepositBooked;
                    }
                    else if (customerOrder.Status is OrderStatus.FullPending || customerOrder.Status is OrderStatus.DepositPending)
                    {
                        vehicle.Status = ElectricVehicleStatus.DealerPending;
                    }
                    else
                    {
                        throw new Exception("Invalid order status for handling order detail.");
                    }
                    _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                }
            }
        }

        public async Task<ResponseDTO> GetAllCustomerOrders(ClaimsPrincipal userClaim, int pageNumber, int pageSize, OrderStatus? orderStatus, CancellationToken ct)
        {
            try
            {
                var userId = userClaim.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found.",
                        StatusCode = 404,
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByManagerOrStaffAsync(userId, ct);
                if (dealer is null)
                {
                    dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                    if (dealer is null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Dealer not found.",
                            StatusCode = 404,
                        };
                    }
                }

                Func<IQueryable<CustomerOrder>, IQueryable<CustomerOrder>> includes = co => co
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.ElectricVehicleVersion)
                                .ThenInclude(v => v.Model)
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.ElectricVehicleColor)
                            .Include(co => co.Quote)
                                .ThenInclude(q => q.QuoteDetails)
                                .ThenInclude(qd => qd.Promotion)
                            .Include(co => co.Customer)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.ElectricVehicleTemplate)
                                .ThenInclude(ev => ev.Color)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.ElectricVehicleTemplate)
                                .ThenInclude(t => t.Version)
                                .ThenInclude(v => v.Model)
                            .Include(co => co.OrderDetails)
                                .ThenInclude(od => od.ElectricVehicle)
                                .ThenInclude(ev => ev.Warehouse)
                            .Include(co => co.EContracts);

                Expression<Func<CustomerOrder, bool>> filter = co => co.Quote.DealerId == dealer.Id;

                (IReadOnlyList<CustomerOrder> items, int total) result;
                result = await _unitOfWork.CustomerOrderRepository.GetPagedAsync(
                            filter: filter,
                            includes: includes,
                            orderBy: dm => dm.CreatedAt,
                            ascending: false,
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            ct: ct);

                var getOrderList = _mapper.Map<List<GetCustomerOrderDTO>>(result.items);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Customer orders retrieved successfully.",
                    StatusCode = 200,
                    Result = new
                    {
                        data = getOrderList,
                        Pagination = new
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalItems = result.total,
                            TotalPages = (int)Math.Ceiling((double)result.total / pageSize)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to get all customer order: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        public async Task<ResponseDTO> CancelCustomerOrderAsync(Guid customerOrderId, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (customerOrder.Status == OrderStatus.Completed || customerOrder.Status == OrderStatus.Cancelled)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Completed or cancelled orders cannot be cancelled.",
                        StatusCode = 400,
                    };
                }

                customerOrder.Status = OrderStatus.Cancelled;
                _unitOfWork.CustomerOrderRepository.Update(customerOrder);

                var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllByCustomerOrderId(customerOrderId, ct);
                {
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        await RestoreVehicleStatus(orderDetails);
                    }
                }

                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Customer order cancelled successfully.",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to cancel: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }

        public async Task RestoreVehicleStatus(List<OrderDetail> orderDetails)
        {
            foreach (var orderDetail in orderDetails)
            {
                var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByIdsAsync(orderDetail.ElectricVehicleId);
                if (vehicle != null)
                {
                    vehicle.Status = ElectricVehicleStatus.AtDealer;
                    _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                }
            }
        }

        public async Task<ResponseDTO> PayDeposit(Guid customerOrderId, bool isCash, CancellationToken ct)
        {
            try
            {
                var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(customerOrderId);
                if (customerOrder is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer order not found.",
                        StatusCode = 404,
                    };
                }

                if (!customerOrder.Status.Equals(OrderStatus.Depositing))
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Only orders with Depositing status can pay deposit.",
                        StatusCode = 400,
                    };
                }

                if (isCash)
                {
                    await _eContractService.CreatePayFullConfirmationEContract(customerOrder.Id, ct);
                    customerOrder.Status = OrderStatus.Completed;
                    _unitOfWork.CustomerOrderRepository.Update(customerOrder);

                    await HandleOrderDetail(customerOrder, ct);

                    var amount = customerOrder.TotalAmount - customerOrder.DepositAmount;
                    var transaction = new Transaction
                    {
                        Amount = amount!.Value,
                        CustomerOrderId = customerOrder.Id,
                        Status = TransactionStatus.Success,
                        OrderRef = customerOrder.OrderNo.ToString(),
                        Currency = "VND",
                        Note = $"Pya remain deposit for order {customerOrder.OrderNo}",
                        Provider = "Cash",
                    };
                    await _unitOfWork.SaveAsync();

                    return new ResponseDTO
                    {
                        IsSuccess = true,
                        Message = "Deposit paid successfully with cash.",
                        StatusCode = 200,
                    };
                }

                var link = await _paymentService.CreateVNPayLink(customerOrder.Id, ct);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "VNPay link for deposit payment created successfully.",
                    StatusCode = 200,
                    Result = link
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Error to pay deposit: {ex.Message}",
                    StatusCode = 500,
                };
            }
        }
    }
}