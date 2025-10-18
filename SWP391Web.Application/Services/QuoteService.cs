using AutoMapper;
using Microsoft.AspNetCore.Http;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Quote;
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
    public class QuoteService : IQuoteService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;

        public QuoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ResponseDTO> CreateQuoteAsync(ClaimsPrincipal user, CreateQuoteDTO createQuoteDTO)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "User not found",
                        StatusCode = 404
                    };
                }

                var dealer = await _unitOfWork.DealerRepository.GetManagerByUserIdAsync(userId, CancellationToken.None);
                if(dealer == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Dealer not found",
                        StatusCode = 404
                    };
                }

                Quote quote = new Quote
                {
                    DealerId = dealer.Id,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = QuoteStatus.Pending,
                    Note = createQuoteDTO.Note,
                    QuoteDetails = new List<QuoteDetail>()
                };

                decimal totalAmount = 0;

                foreach( var dt in createQuoteDTO.QuoteDetails)
                {
                    // take version
                    var version = await _unitOfWork.ElectricVehicleVersionRepository.GetByIdsAsync(dt.VersionId);
                    if (version == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No version found ",
                            StatusCode = 404
                        };
                    }

                    //take color
                    var color = await _unitOfWork.ElectricVehicleColorRepository.GetByIdsAsync(dt.ColorId);
                    if(color == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " No color found ",
                            StatusCode = 404
                        };
                    }

                    var warehouse = await _unitOfWork.WarehouseRepository.GetWarehouseByDealerIdAsync(dealer.Id);
                    if(warehouse == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Warehouse 's dealer is not found ",
                            StatusCode = 404
                        };
                    }

                    var vehicle = await _unitOfWork.ElectricVehicleRepository.GetByVersionColorAndWarehouseAsync(version.Id , color.Id , warehouse.Id);
                    if(vehicle == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No vehicle found",
                            StatusCode = 404
                        };
                    }

                    // get promotion 
                    var promotion = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(dt.PromotionId);
                    if (promotion == null)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Promotion not found",
                            StatusCode = 404
                        };
                    }

                    if (!promotion.IsActive || promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "Promotion not active",
                            StatusCode = 404
                        };
                    }

                    decimal basePrice = vehicle.CostPrice;
                    decimal extraPrice = color.ExtraCost;
                    decimal unitPrice = basePrice + extraPrice;
                    decimal discount = 0;

                    if ((promotion.ModelId == null && promotion.VersionId == null) 
                        || (promotion.ModelId == version.ModelId && promotion.VersionId == version.Id))
                    {
                        if(promotion.DiscountType == DiscountType.Percentage && promotion.Percentage.HasValue)
                        {
                            discount = (unitPrice * promotion.Percentage.Value) / 100;
                        }
                        else if(promotion.DiscountType == DiscountType.FixAmount && promotion.FixedAmount.HasValue)
                        {
                            discount = promotion.FixedAmount.Value;
                        }
                    }

                    decimal totalPrice = (unitPrice - discount) * dt.Quantity;

                    totalAmount += totalPrice;

                    var quoteDetail = new QuoteDetail
                    {
                        VersionId = version.Id,
                        ColorId = color.Id,
                        Quantity = dt.Quantity,
                        PromotionId = dt.PromotionId,
                        UnitPrice = unitPrice,
                        Promotion = promotion,
                        TotalPrice = totalPrice,
                    };
                    quote.QuoteDetails.Add(quoteDetail);
                }
                quote.TotalAmount = totalAmount;

                await _unitOfWork.QuoteRepository.AddAsync(quote, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                var quoteDTO = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(quote.Id);
                var getQuoteDTO = _mapper.Map<GetQuoteDTO>(quoteDTO);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create quote successfully",
                    StatusCode = 200,
                    Result = getQuoteDTO
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

        public async Task<ResponseDTO> GetAllAsync(ClaimsPrincipal user)
        {
            try
            {
                var quote = await _unitOfWork.QuoteRepository.GetAllAsync();
                var getQuote = _mapper.Map<List<GetQuoteDTO>>(quote);
                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Quotes retrieve successfully",
                    StatusCode = 200,
                    Result = getQuote
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500,
                };
            }

        }

        public async Task<ResponseDTO> GetQuoteByIdAsync(ClaimsPrincipal user,  Guid id)
        {
            try
            {
                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(id);
                if (quote == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found",
                        StatusCode = 404,
                    };
                }

                var getQuote = _mapper.Map<List<GetQuoteDTO>>(quote);

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Quote retrieved successfully",
                    StatusCode = 200,
                    Result = getQuote
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

        public async Task<ResponseDTO> UpdateQuoteStatusAsync(Guid id, QuoteStatus newStatus)
        {
            try
            {
                var quote = await _unitOfWork.QuoteRepository.GetQuoteByIdAsync(id);
                if (quote == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Quote not found",
                        StatusCode = 404,
                    };
                }

                quote.Status = newStatus;
                _unitOfWork.QuoteRepository.Update(quote);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Quote retrieved successfully",
                    StatusCode = 200,
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
