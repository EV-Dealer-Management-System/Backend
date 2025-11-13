using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Repository
{
    public class DealerDailyInventoryRepository : IDealerDailyInventoryRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerDailyInventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DealerDailyInventory?> GetAsync(Guid dealerId, Guid evTemplateId, DateTime snapshotDate, CancellationToken ct)
        {
            var dateOnly = snapshotDate.Date;
            return await _context.DealerDailyInventories
                .FirstOrDefaultAsync(x => x.DealerId == dealerId
                                       && x.EVTemplateId == evTemplateId
                                       && x.SnapshotDate == dateOnly, ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetClosingStockMapAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == snapshotDateUtc.Date)
                .ToDictionaryAsync(
                    keySelector: x => (x.DealerId, x.EVTemplateId),
                    elementSelector: x => x.ClosingStock,
                    cancellationToken: ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetInflowAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            var dateOnly = snapshotDateUtc.Date;
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == dateOnly)
                .ToDictionaryAsync(
                    x => (x.DealerId, x.EVTemplateId),
                    x => x.Inflow,
                    ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOpeningStockAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == snapshotDateUtc.Date)
                .ToDictionaryAsync(
                    keySelector: x => (x.DealerId, x.EVTemplateId),
                    elementSelector: x => x.OpeningStock,
                    cancellationToken: ct);
        }

        public async Task<Dictionary<(Guid DealerId, Guid EVTemplateId), int>> GetOutflowAsync(DateTime snapshotDateUtc, CancellationToken ct)
        {
            var dateOnly = snapshotDateUtc.Date;
            return await _context.DealerDailyInventories
                .AsNoTracking()
                .Where(x => x.SnapshotDate == dateOnly)
                .ToDictionaryAsync(
                    x => (x.DealerId, x.EVTemplateId),
                    x => x.Outflow,
                    ct);
        }

        public async Task UpsertRangeAsync(IEnumerable<DealerDailyInventory> rows, CancellationToken ct)
        {
            foreach (var r in rows)
            {
                var existing = await _context.DealerDailyInventories
                    .FirstOrDefaultAsync(x => x.DealerId == r.DealerId
                                           && x.EVTemplateId == r.EVTemplateId
                                           && x.SnapshotDate == r.SnapshotDate.Date, ct);

                if (existing is null)
                {
                    await _context.DealerDailyInventories.AddAsync(r, ct);
                }
                else
                {
                    existing.OpeningStock = r.OpeningStock;
                    existing.Inflow = r.Inflow;
                    existing.Outflow = r.Outflow;
                    existing.ClosingStock = r.ClosingStock;
                    existing.Note = r.Note;
                    _context.DealerDailyInventories.Update(existing);
                }
            }
        }
    }
}
