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
    public class BookingDetailRepository : Repository<BookingEVDetail>, IBookingDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookingEVDetail>> GetBookingDetailsByBookingIdAsync(Guid bookingId, CancellationToken ct)
        {
            return await _context.BookingEVDetails
                .Where(bd => bd.BookingId == bookingId)
                .ToListAsync(ct);
        }
    }
}
