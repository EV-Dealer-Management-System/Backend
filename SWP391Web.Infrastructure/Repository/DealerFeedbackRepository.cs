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
    public class DealerFeedbackRepository : Repository<DealerFeedback>, IDealerFeedbackRepository
    {
        public readonly ApplicationDbContext _context;
        public DealerFeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        
        public async Task<DealerFeedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await _context.DealerFeedbacks
                .FirstOrDefaultAsync(dfb => dfb.Id == id);
        }

        public async Task<List<DealerFeedback>> GetFeedbacksByDealerIdAsync(Guid dealerId)
        {
            return await _context.DealerFeedbacks
                .Where(dfb => dfb.DealerId == dealerId)
                .ToListAsync();
        }
    }
}
