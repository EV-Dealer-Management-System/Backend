using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class EContractTermRepository : Repository<EContractTerm>, IEContractTermRepository
    {
        private readonly ApplicationDbContext _context;
        public EContractTermRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<EContractTerm?> GetByLevelAsync(int level, CancellationToken ct)
        {
            return await _context.EContractTerms
                .Where(et => et.DealerLevel == level)
                .FirstOrDefaultAsync(ct);
        }
    }
}
