using AutoMapper;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerTier;
using SWP391Web.Application.IServices;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class DealerTierService : IDealerTierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DealerTierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseDTO> GetAllDealerTiers(CancellationToken ct)
        {
            try
            {
                var dealerTiers = (await _unitOfWork.DealerTierRepository.GetAllAsync()).OrderByDescending(dt => dt.Level);

                var getdealerTiers = _mapper.Map<List<GetDealerTierDTO>>(dealerTiers);
                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Dealer Tiers retrieved successfully.",
                    Result = getdealerTiers
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"An error occurred while retrieving Dealer Tiers: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> UpdateDealerTier(Guid dealerTierId, UpdateDealerTierDTO updateDealer, CancellationToken ct)
        {
            try
            {
                var dealerTier = await _unitOfWork.DealerTierRepository.GetByIdAsync(dealerTierId, ct);
                if (dealerTier is null)
                {
                    return new ResponseDTO
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Dealer Tier not found."
                    };
                }

                dealerTier.Name = updateDealer.Name ?? dealerTier.Name;
                dealerTier.Description = updateDealer.Description ?? dealerTier.Description;

                if (updateDealer.Level.HasValue)
                    dealerTier.Level = updateDealer.Level.Value;

                if (updateDealer.BaseCommissionPercent.HasValue)
                    dealerTier.BaseCommissionPercent = updateDealer.BaseCommissionPercent.Value;

                if (updateDealer.BaseDepositPercent.HasValue)
                    dealerTier.BaseDepositPercent = updateDealer.BaseDepositPercent.Value;

                if (updateDealer.BaseLatePenaltyPercent.HasValue)
                    dealerTier.BaseLatePenaltyPercent = updateDealer.BaseLatePenaltyPercent.Value;

                if (updateDealer.BaseCreditLimit.HasValue)
                    dealerTier.BaseCreditLimit = updateDealer.BaseCreditLimit.Value;

                dealerTier.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.DealerTierRepository.Update(dealerTier);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Dealer Tier updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = $"An error occurred while updating the Dealer Tier: {ex.Message}"
                };
            }
        }
    }
}
