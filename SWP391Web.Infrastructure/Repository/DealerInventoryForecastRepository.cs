using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class DealerInventoryForecastRepository : Repository<DealerInventoryForecast>, IDealerInventoryForecastRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerInventoryForecastRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task UpsertRangeAsync(IEnumerable<DealerInventoryForecast> rows, CancellationToken ct)
        {
            foreach (var r in rows)
            {
                var existing = await _context.DealerInventoryForecasts
                    .FirstOrDefaultAsync(x =>
                        x.DealerId == r.DealerId &&
                        x.EVTemplateId == r.EVTemplateId &&
                        x.TargetDate == r.TargetDate.Date,
                        ct);

                if (existing is null)
                {
                    await _context.DealerInventoryForecasts.AddAsync(r, ct);
                }
                else
                {
                    existing.Forecast = r.Forecast;
                    existing.ForecastLower = r.ForecastLower;
                    existing.ForecastUpper = r.ForecastUpper;
                    existing.ModelVersion = r.ModelVersion;
                    existing.CreatedAtUtc = r.CreatedAtUtc;
                    _context.DealerInventoryForecasts.Update(existing);
                }
            }
        }

        public async Task<IEnumerable<DealerInventoryForecast>> GetRangeAsync(Guid dealerId, Guid evTemplateId, DateTime fromDate, DateTime toDate, CancellationToken ct)
        {
            var from = fromDate.Date;
            var to = toDate.Date;

            return await _context.DealerInventoryForecasts
                .AsNoTracking()
                .Where(x =>
                    x.DealerId == dealerId &&
                    x.EVTemplateId == evTemplateId &&
                    x.TargetDate >= from &&
                    x.TargetDate <= to)
                .OrderBy(x => x.TargetDate)
                .ToListAsync(ct);
        }
    }
}
