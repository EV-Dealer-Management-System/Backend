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
    public class DiscountRepository : Repository<Discount>, IDiscountRepository
    {
        private readonly ApplicationDbContext _context;
        public DiscountRepository(ApplicationDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Discount?> GetByCodeAsync(string code)
        {
            return await _context.Discounts
                .Include(d => d.Code)
                .FirstOrDefaultAsync(d => d.Code == code);
        }

        public async Task<Discount?> GetByIdAsync(Guid discountId)
        {
            return await _context.Discounts
                .Include(d => d.Id)
                .FirstOrDefaultAsync(d => d.Id == discountId);
        }

        public Task<bool> IsExistByCode(string code)
        {
            return _context.Discounts
                .AnyAsync(d => d.Code == code);
        }

        public Task<bool> IsExistByCodeExceptId(string code, Guid exceptId)
        {
            return _context.Discounts
                .AnyAsync(d => d.Code == code && d.Id != exceptId);
        }
    }
}
