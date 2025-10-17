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

                Quote qoute = new Quote
                {
                    DealerId = dealer.Id,
                    CreatedBy = dealer.Name,
                    CreatedAt = DateTime.UtcNow,
                    Status = QuoteStatus.Pending,
                    Note = createQuoteDTO.Note,
                };

                decimal totalAmount = 0;

                foreach( var dt in createQuoteDTO.QuoteDetails)
                {
                    //Get vehicle in dealer's inventory
                    var availableVehicles = await _unitOfWork.ElectricVehicleRepository
                        .GetAvailableVehicleByDealerAsync(dealer.Id, dt.VersionId, dt.ColorId);
                    if(availableVehicles == null || !availableVehicles.Any())
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = "No available in dealer 's inventory",
                            StatusCode = 404
                        };
                    }

                    if(availableVehicles.Count() < dt.Quantity)
                    {
                        return new ResponseDTO
                        {
                            IsSuccess = false,
                            Message = " Not enough vehicle in dealer 's inventory",
                            StatusCode = 404
                        };
                    }

                    //Take vehicle with ImportDate latest
                    var selectedVehicles = availableVehicles
                        .OrderBy(ev => ev.ImportDate)
                        .Take(dt.Quantity)
                        .ToList();

                    var basePrice = selectedVehicles.First().CostPrice;
                    decimal discount = 0;

                    if (dt.PromotionId.HasValue)
                    {
                        var promo = await _unitOfWork.PromotionRepository.GetPromotionByIdAsync(dt.PromotionId.Value);
                    }
                }

                await _unitOfWork.QuoteRepository.AddAsync(qoute, CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Create quote successfully",
                    StatusCode = 200,
                    Result = qoute
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
