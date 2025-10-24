using AutoMapper;
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
        public CustomerOrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateCustomerOrderAsync(ClaimsPrincipal user, CreateCustomerOrderDTO createCustomerOrderDTO)
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

                var customerOrder = new CustomerOrder
                {
                    CustomerId = createCustomerOrderDTO.CustomerId,
                    QuoteId = quote.Id,
                    OrderNo = createCustomerOrderDTO.OrderNo,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = quote.TotalAmount,
                    Status = OrderStatus.Pending,
                };

                await _unitOfWork.CustomerOrderRepository.AddAsync(customerOrder,CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var getCustomerOrder = _mapper.Map<GetCustomerOrderDTO>(customerOrder);

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
    }
}
