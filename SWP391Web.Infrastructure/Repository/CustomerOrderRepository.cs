using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Domain.Enums;
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
        public int GenerateOrderNumber()
        {
            return _context.CustomerOrders.Count() + 1;
        }

        public async Task<CustomerOrder?> GetByIdAsync(Guid customerOrderId)
        {
            return await _context.CustomerOrders
                .Include(c => c.Quote)
                    .ThenInclude(q => q.QuoteDetails)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Dealer)
                        .ThenInclude(d => d.Manager)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == customerOrderId);
        }

        public async Task<bool>? IsExistByIdAsync(Guid id)
        {
            return await _context.CustomerOrders.AnyAsync(c => c.Id == id);
        }

        public async Task<CustomerOrder?> GetByOrderNoAsync(int customerOrderNo)
        {
            return await _context.CustomerOrders
                .AsTracking()
                .Include(co => co.OrderDetails)
                .Include(co => co.Quote)
                .FirstOrDefaultAsync(c => c.OrderNo == customerOrderNo);
        }

        public async Task<CustomerOrder?> GetByEContractId(Guid eContractId, CancellationToken ct)
        {
            return await _context.CustomerOrders
                .Include(co => co.EContracts)
                .Include(co => co.Quote)
                .Include(co => co.Customer)
                .FirstOrDefaultAsync(co => co.EContracts!.Any(ec => ec.Id == eContractId), ct);
        }
    }
}
