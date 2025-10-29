using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.CustomerOrder;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public CustomerOrderService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService, IDepositSettingService depositSetting)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _paymentService = paymentService;
            _depositSetting = depositSetting;
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
                        Message = "Quote is not accepted yet . Cann't create order.",
                        StatusCode = 400,
                    };
                }

                var orderNo = _unitOfWork.CustomerOrderRepository.GenerateOrderNumber();

                OrderStatus status;
                var amount = quote.TotalAmount;
                if (createCustomerOrderDTO.IsPayFull)
                {
                    status = OrderStatus.FullPending;
                }
                else
                {
                    status = OrderStatus.DepositPending;
                    var depositRate = await _depositSetting.GetDepositSetting(user, ct);
                    amount = quote.TotalAmount * (depositRate.Data!.MaxDepositPercentage / 100);
                }

                var customerOrder = new CustomerOrder
                {
                    CustomerId = createCustomerOrderDTO.CustomerId,
                    QuoteId = quote.Id,
                    OrderNo = orderNo,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = (int)amount,
                    Status = status,
                };

                await _unitOfWork.CustomerOrderRepository.AddAsync(customerOrder, ct);

                await HandleOrderDetail(quote, ct);

                await _unitOfWork.SaveAsync();

                var getCustomerOrder = _mapper.Map<GetCustomerOrderDTO>(customerOrder);

                await _paymentService.CreateVNPayLink(customerOrder.Id, ct);

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

        private async Task HandleOrderDetail(Quote quote, CancellationToken ct)
        {
            foreach (var quoteDetail in quote.QuoteDetails)
            {
                var vehicles = await _unitOfWork.ElectricVehicleRepository
                    .GetVehicleByQuantityWithOldestImportDateForDealerAsync(
                        quoteDetail.VersionId,
                        quoteDetail.ColorId,
                        quote.Dealer.Warehouse.Id,
                        quoteDetail.Quantity);

                foreach (var vehicle in vehicles)
                {
                    var orderDetail = new OrderDetail
                    {
                        CustomerOrderId = quote.CustomerOrders.First().Id,
                        ElectricVehicleId = vehicle.Id
                    };
                    await _unitOfWork.OrderDetailRepository.AddAsync(orderDetail, ct);
                    vehicle.Status = ElectricVehicleStatus.DealerPending;
                    _unitOfWork.ElectricVehicleRepository.Update(vehicle);
                }
            }
        }

        public Task<ResponseDTO> GetCustomerOrderByIdAsync(Guid customerOrderId, CancellationToken ct)
        {
            throw new NotImplementedException();
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
                                .ThenInclude(qd => qd.Promotion);

                (IReadOnlyList<CustomerOrder> items, int total) result;
                result = await _unitOfWork.CustomerOrderRepository.GetPagedAsync(
                            filter: null,
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
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }
        }
    }
}
