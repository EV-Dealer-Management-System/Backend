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
        public async Task<ResponseDTO> CreateQuoteAsync(CreateQuoteDTO createQuoteDTO)
        {
            try
            {
                Quote qoute = new Quote
                {
                    WarehouseId = createQuoteDTO.WarehouseId,
                    DealerId = createQuoteDTO.DealerId,
                    CreatedById = createQuoteDTO.CreatedById,
                    CreatedAt = DateTime.UtcNow,
                    Status = QuoteStatus.Pending,
                    TotalAmount = createQuoteDTO.TotalAmount,
                    Note = createQuoteDTO.Note,
                };

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

        public async Task<ResponseDTO> GetAllAsync()
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

        public async Task<ResponseDTO> GetQuoteByIdAsync(Guid id)
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
