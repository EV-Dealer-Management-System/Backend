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
    public class CustomerFeedbackRepository : Repository<CustomerFeedback>, ICustomerFeedbackRepository
    {
        public readonly ApplicationDbContext _context;
        public CustomerFeedbackRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<List<CustomerFeedback>> GetAllCustomerFeedbacksWithDetailAsync(CancellationToken ct)
        {
            return await _context.CustomerFeedbacks
                .Include(cf => cf.Customer)
                .Include(cf => cf.Dealer)
                .Include(cf => cf.CustomerFBAttachments)
                .OrderByDescending(cf => cf.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<CustomerFeedback?> GetFeedbackByIdAsync(Guid id)
        {
            return await _context.CustomerFeedbacks
                .Include(cf => cf.Customer)
                .Include(cf => cf.Dealer)
                .Include(cf => cf.CustomerFBAttachments)
                .FirstOrDefaultAsync(cf => cf.Id == id);
        }

        public Task<List<CustomerFeedback>> GetFeedbacksByCustomerIdAsync(Guid customerId)
        {
            return _context.CustomerFeedbacks
                .Where(cf => cf.CustomerId == customerId)
                .Include(cf => cf.Dealer)
                .Include(cf => cf.CustomerFBAttachments)
                .OrderByDescending(cf => cf.CreatedAt)
                .ToListAsync();
        }

        public Task<List<CustomerFeedback>> GetFeedbacksByDealerIdAsync(Guid dealerId)
        {
            return _context.CustomerFeedbacks
                .Where(cf => cf.DealerId == dealerId)
                .Include(cf => cf.Customer)
                .Include(cf => cf.CustomerFBAttachments)
                .OrderByDescending(cf => cf.CreatedAt)
                .ToListAsync();
        }
    }
}
