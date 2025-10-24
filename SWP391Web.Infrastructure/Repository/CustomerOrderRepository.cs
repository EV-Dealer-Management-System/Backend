using Microsoft.EntityFrameworkCore;
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

        public async Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId)
        {
            return await _context.CustomerOrders
                .FirstOrDefaultAsync(c => c.Id == customerOrderId);
        }

        public async Task<bool>? IsExistByIdAsync(Guid id)
        {
            return await _context.CustomerOrders.AnyAsync(c => c.Id == id);
        }
    }
}
