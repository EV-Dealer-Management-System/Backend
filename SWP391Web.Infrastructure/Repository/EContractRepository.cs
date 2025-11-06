using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
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
            return await _context.EContracts
                .Include(ec => ec.BookingEV)
                .Where(x => x.Id == id).FirstOrDefaultAsync(ct);
        }

        public async Task<List<EContract>?> GetEContractDealerByDealerIdAsync(string managerId, CancellationToken ct)
        {
            return await _context.EContracts
                .Include(ec => ec.Owner)
                .Where(ec => ec.Type == EcontractType.DealerContract && ec.OwnerBy == managerId)
                .ToListAsync(ct);
        }

        public async Task<EContract?> GetByBookingId(Guid bookingId, CancellationToken ct)
        {
            return await _context.EContracts
                .Include(ec => ec.BookingEV)
                .Where(ec => ec.BookingEV != null && ec.BookingEV.Id == bookingId && ec.Type == EcontractType.BookingContract)
                .FirstOrDefaultAsync(ct);
        }
    }
}
