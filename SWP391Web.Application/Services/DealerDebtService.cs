using Aspose.Words;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerDebt;
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
    public class DealerDebtService : IDealerDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDealerDebtTransactionService _dealerDebtTransactionService;
        private readonly IMapper _mapper;
        public DealerDebtService(IUnitOfWork unitOfWork, IDealerDebtTransactionService dealerDebtTransactionService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _dealerDebtTransactionService = dealerDebtTransactionService;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> AddPurchaseForDealerAsync(Guid dealerId, RecordDebtDTO debtDTO, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(debtDTO.ConfirmDateUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Purchase,
                    Amount = debtDTO.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("PURCHASE", debtDTO.ReferenceNo, dealerId, now),
                    SourceType = debtDTO.SourceType,
                    SourceId = debtDTO.SourceId,
                    SourceNo = debtDTO.ReferenceNo,
                    ReferenceNo = debtDTO.ReferenceNo,
                    Note = debtDTO.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 201,
                    IsSuccess = true,
                    Message = "Successfully added purchase for dealer debt.",
                    Result = create
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"Failed to add purchase for dealer debt: {ex.Message}"
                };
            }
        }

        private static string BuildExtId(string kind, string? referenceNo, Guid dealerId, DateTime now)
        {
            if (!string.IsNullOrWhiteSpace(referenceNo))
                return $"{kind}:{referenceNo}".Trim();

            return $"{kind}:{dealerId}:{now:yyyyMMddHHmmss}";
        }

        public async Task<ResponseDTO> AddPaymentForDealerAsync(Guid dealerId, RecordPaymentDTO paymentDTO, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(paymentDTO.PaidAtUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Payment,
                    Amount = paymentDTO.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("PAYMENT", paymentDTO.ReferenceNo, dealerId, now),
                    Method = paymentDTO.Method,
                    SourceType = paymentDTO.SourceType,
                    SourceId = paymentDTO.SourceId,
                    SourceNo = paymentDTO.ReferenceNo,
                    ReferenceNo = paymentDTO.ReferenceNo,
                    Note = paymentDTO.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);

                return new ResponseDTO(true)
                {
                    StatusCode = 200,
                    Message = "Successfully recorded payment for dealer debt."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO(false)
                {
                    StatusCode = 500,
                    Message = $"Failed to record payment: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> AddCommissionForDealerAsync(Guid dealerId, RecordCommissionDTO dto, CancellationToken ct)
        {
            try
            {
                var now = DateTime.SpecifyKind(dto.AtUtc, DateTimeKind.Utc);

                var create = new CreateDealerDebtTransactionDTO
                {
                    DealerId = dealerId,
                    Type = DealerDebtTransactionType.Commission,
                    Amount = dto.Amount,
                    OccurredAtUtc = now,
                    ExternalId = BuildExtId("COMMISSION", dto.ReferenceNo, dealerId, now),
                    SourceType = dto.SourceType,
                    SourceId = dto.SourceId,
                    SourceNo = dto.ReferenceNo,
                    ReferenceNo = dto.ReferenceNo,
                    Note = dto.Note
                };

                await _dealerDebtTransactionService.CraeteDealerDebtTransaction(create, ct);
                await _unitOfWork.SaveAsync();
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Successfully recorded commission for dealer."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO(false)
                {
                    StatusCode = 500,
                    Message = $"Failed to record commission: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDealerDebtBalanceAtQuarterNow(Guid dealerId, CancellationToken ct)
        {
            try
            {
                var dealer = await _unitOfWork.DealerRepository.GetByIdAsync(dealerId, ct);
                if (dealer is null)
                {
                    return new ResponseDTO(false)
                    {
                        StatusCode = 404,
                        Message = "Dealer not found."
                    };
                }

                var dealerDebt = await _unitOfWork.DealerDebtRepository.GetOrCreateQuarterAsync(dealerId, DateTime.Now, ct);
                if (dealerDebt is null)
                {
                    return new ResponseDTO(false)
                    {
                        StatusCode = 404,
                        Message = "Dealer debt record not found."
                    };
                }

                var getDealerDebt = _mapper.Map<GetDealerDebtDTO>(dealerDebt);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    Message = "Successfully retrieved dealer debt balance.",
                    Result = getDealerDebt
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO(false)
                {
                    StatusCode = 500,
                    Message = $"Failed to retrieve dealer debt balance: {ex.Message}"
                };
            }
        }

        public Task<ResponseDTO> GetDealerDebtDetails(Guid dealerId, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
