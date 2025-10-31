using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerTier;
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
    }
}
