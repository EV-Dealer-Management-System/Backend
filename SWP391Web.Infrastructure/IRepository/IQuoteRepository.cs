using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IQuoteRepository : IRepository<Quote>
    {
        Task<Quote?> GetQuoteByIdAsync(Guid quoteId);
        Task<bool> IsQuoteExistByIdAsync(Guid quoteId);
        Task<List<Quote>> GetAllQuotesWithDetailAsync();
    }
}
