using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class EContractRepository : Repository<EContract>, IEContractRepository
    {
        private readonly ApplicationDbContext _context;
        public EContractRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EContract?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.EContracts.Where(x => x.Id == id).FirstOrDefaultAsync(ct);
        }
    }
}
