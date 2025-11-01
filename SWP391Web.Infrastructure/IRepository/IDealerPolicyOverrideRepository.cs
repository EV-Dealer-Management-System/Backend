using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerPolicyOverrideRepository : IRepository<DealerPolicyOverride>
    {
        Task<DealerPolicyOverride?> GetActiveByDealerAsync(Guid dealerId, CancellationToken ct);
        Task<DealerPolicyOverride?> GetCurrentActiveAsync(Guid dealerId, DateTime now, CancellationToken ct);
    }
}
