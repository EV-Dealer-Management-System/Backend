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
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByCustomerOrderIdAsync(Guid customerOrderId, CancellationToken ct)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.CustomerOrderId == customerOrderId, ct);
        }

        public async Task<bool> IsExistTransactionAsync(string method, string orderRef, CancellationToken ct)
        {
            return await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.Provider == method && t.OrderRef == orderRef, ct);
        }
    }
}
