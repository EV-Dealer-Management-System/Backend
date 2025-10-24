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
        private readonly IUnitOfWork _unitOfWork;
        public CustomerOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateOrderDTO createOrderDTO, CancellationToken ct)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new ResponseDTO(false)
                    {
                        Message = "User not login yet",
                        StatusCode = 401
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetDealerByUserIdAsync(userId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO(false)
                    {
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(createOrderDTO.QuoteId);
                if (quote is null)
                {
                    return new ResponseDTO(false)
                    {
                        Message = "Quote not found",
                        StatusCode = 404
                    };
                }

                var customer = await _unitOfWork.CustomerRepository.GetByIdAsync(createOrderDTO.CustomerId);

                if (customer is null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Customer not found",
                        StatusCode = 404
                    };
                }

                var orderNo = _unitOfWork.CustomerOrderRepository.GenerateOrderNumber();
                var order = new CustomerOrder
                {
                    CustomerId = customer.Id,
                    QuoteId = createOrderDTO.QuoteId,
                    OrderNo = orderNo,
                    TotalAmount = quote.TotalAmount
                };

                foreach (var quoteDetail in quote.QuoteDetails)
                {
                    var vehicles = await _unitOfWork.ElectricVehicleRepository.GetVehicleByQuantityWithOldestImportDateForDealerAsync(quoteDetail.VersionId, quoteDetail.ColorId, dealer.Warehouse.Id, quoteDetail.Quantity);
                    foreach (var ev in vehicles)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ElectricVehicleId = ev.Id,
                            CustomerOrderId = order.Id,
                        };

                        order.OrderDetails.Add(orderDetail);

                        ev.Status = ElectricVehicleStatus.DealerPending;
                        _unitOfWork.ElectricVehicleRepository.Update(ev);
                    }
                }

                await _unitOfWork.CustomerOrderRepository.AddAsync(order, ct);

                var result = await _unitOfWork.SaveAsync();

                if (result > 0)
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = true,
                        Message = "Create order successfully",
                        StatusCode = 201
                    };
                }
                else
                {
                    return new ResponseDTO()
                    {
                        IsSuccess = false,
                        Message = "Failed to create order",
                        StatusCode = 500
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = $"Error to create a order: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
