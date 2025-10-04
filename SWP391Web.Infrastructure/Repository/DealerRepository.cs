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
    public class DealerRepository : Repository<Dealer>, IDealerRepository
    {
        private readonly ApplicationDbContext context;
        public DealerRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Dealer?> GetByIdAsync(Guid dealerId, CancellationToken ct)
        {
            return await context.Dealers
                .Where(dl => dl.Id == dealerId).FirstOrDefaultAsync(ct);
        }

        public Task<ApplicationUser?> GetManagerByDealerId(Guid dealerId, CancellationToken ct)
        {
            return context.Dealers
                .AsNoTracking()
                .Where(dl => dl.Id == dealerId)
                .Select(dl => dl.Manager)
                .SingleOrDefaultAsync(ct);
        }

        public async Task<bool> IsExistByNameAsync(string name, CancellationToken ct)
        {
            return await context.Dealers
                .AnyAsync(dl => dl.Name == name, ct);
        }
    }
}
