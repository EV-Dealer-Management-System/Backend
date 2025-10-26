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
    public class DepositSettingRepository : Repository<DepositSetting>, IDepositSettingRepository
    {
        private readonly ApplicationDbContext _context;
        public DepositSettingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DepositSetting?> GetByDealerIdAsync(Guid dealerId, CancellationToken ct)
        {
            return await _context.DepositSettings
                .Include(ds => ds.Dealer)
                .Include(ds => ds.Manager)
                .FirstOrDefaultAsync(ds => ds.DealerId == dealerId, ct);
        }

        public async Task<DepositSetting?> GetByDefaultAsync(CancellationToken ct)
        {
            return await _context.DepositSettings
                .Include(ds => ds.Dealer)
                .Include(ds => ds.Manager)
                .FirstOrDefaultAsync(ds => ds.DealerId == null, ct);
        }

        public async Task<DepositSetting?> GetByUserIdAsync(string userId, CancellationToken ct)
        {
            return await _context.DepositSettings
                .Include(ds => ds.Dealer)
                .Include(ds => ds.Manager).
                FirstOrDefaultAsync(ds => ds.ManagerId == userId, ct);
        }
    }
}
