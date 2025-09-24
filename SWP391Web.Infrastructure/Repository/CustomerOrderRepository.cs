using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Context;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Infrastructure.Repository
{
    public class CustomerOrderRepository : Repository<CustomerOrder>, ICustomerOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public CustomerOrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CustomerOrder?> GetByIdAsync(Guid customerId)
        {
            return await _context.CustomerOrders.FindAsync(customerId);
        }
    }
}
