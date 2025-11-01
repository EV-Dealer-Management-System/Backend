using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.Dealer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IDealerTierService
    {
        Task<ResponseDTO> UpdateDealerTier(Guid dealerTierId, UpdateDealerTierDTO updateDealer, CancellationToken ct);
        Task<ResponseDTO> GetAllDealerTiers(CancellationToken ct);
        Task<ResponseDTO> CreateDealerPolicyOverrideAsync(Guid dealerId, CreateDealerPolicyOverrideDTO createDealerPolicy, CancellationToken ct);
        Task<DealerEffectivePolicyDTO> GetEffectivePolicyAsync(Guid dealerId, CancellationToken ct);
    }
}
