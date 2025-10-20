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
    public class QuoteRepository : Repository<Quote> , IQuoteRepository
    {
        public readonly ApplicationDbContext _context;
        public QuoteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Quote>> GetAllQuotesWithDetailAsync()
        {
            return await _context.Quotes
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleVersion)
                        .ThenInclude(v => v.Model)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleColor)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.Promotion)
                .Include(q => q.Dealer)
                .ToListAsync();
        }

        public async Task<Quote?> GetQuoteByIdAsync(Guid quoteId)
        {
            return await _context.Quotes
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleVersion)
                        .ThenInclude(v => v.Model)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.ElectricVehicleColor)
                .Include(q => q.QuoteDetails)
                    .ThenInclude(qd => qd.Promotion)
                .Include(q => q.Dealer)
                .FirstOrDefaultAsync(q => q.Id == quoteId);
        }

        public async Task<bool> IsQuoteExistByIdAsync(Guid quoteId)
        {
            return await _context.Quotes.AnyAsync(q => q.Id == quoteId);
        }
    }
}
