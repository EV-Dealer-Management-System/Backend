using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Customer?> GetByUserIdAsync(string userId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();
        }
    }
}
