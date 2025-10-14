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


        public async Task<Quote?> GetQuoteByIdAsync(Guid quoteId)
        {
            return await _context.Quotes.FirstOrDefaultAsync(q => q.Id == quoteId);
        }

        public async Task<bool> IsQuoteExistByIdAsync(Guid quoteId)
        {
            return await _context.Quotes.AnyAsync(q => q.Id == quoteId);
        }
    }
}
