using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerTierRepository : IRepository<DealerTier>
    {
        Task<DealerTier?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<DealerTier?> GetByLevelAsync(int level, CancellationToken ct);
    }
}
