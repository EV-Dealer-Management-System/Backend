using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerDebtRepository : IRepository<DealerDebt>
    {
        Task<DealerDebt?> GetCurrentQuarterAsync(Guid dealerId, DateTime asOf, CancellationToken ct);
        Task<DealerDebt> CreateQuarterAsync(Guid dealerId, DateTime asOf, decimal openingBalance, CancellationToken ct);
        Task<DealerDebt?> GetByDealerAndPeriodAsync(Guid dealerId, DateTime periodFrom, DateTime periodTo, CancellationToken ct);
        (DateTime from, DateTime to) GetQuarterRangeUtc(DateTime asOfUtc);
        Task<DealerDebt?> GetLatestByDealerAsync(Guid dealerId, CancellationToken ct);
    }
}
