using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class DealerDebtRepository : Repository<DealerDebt>, IDealerDebtRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerDebtRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public (DateTime from, DateTime to) GetQuarterRangeUtc(DateTime asOfUtc)
        {
            var dt = DateTime.SpecifyKind(asOfUtc, DateTimeKind.Utc);

            var q = (dt.Month - 1) / 3 + 1;
            int startMonth = (q - 1) * 3 + 1;

            var from = new DateTime(dt.Year, startMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(3).AddTicks(-1);

            return (from, to);
        }

        public async Task<DealerDebt?> GetCurrentQuarterAsync(Guid dealerId, DateTime asOf, CancellationToken ct)
        {
            var (from, to) = GetQuarterRangeUtc(asOf);
            return await _context.DealerDebts
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DealerId == dealerId &&
                    d.PeriodFrom == from &&
                    d.PeriodTo == to, ct);
        }

        public async Task<DealerDebt?> GetByDealerAndPeriodAsync(Guid dealerId, DateTime periodFrom, DateTime periodTo, CancellationToken ct)
        {
            return await _context.DealerDebts
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.DealerId == dealerId &&
                    d.PeriodFrom == periodFrom &&
                    d.PeriodTo == periodTo, ct);
        }

        public Task<DealerDebt> CreateQuarterAsync(Guid dealerId, DateTime asOf, decimal openingBalance, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<DealerDebt?> GetLatestByDealerAsync(Guid dealerId, CancellationToken ct)
        {
            return await _context.DealerDebts
                .AsNoTracking()
                .Where(d => d.DealerId == dealerId)
                .OrderByDescending(d => d.PeriodTo)
                .FirstOrDefaultAsync(ct);
        }
    }
}
