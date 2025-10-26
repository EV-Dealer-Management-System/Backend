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

        public async Task<Customer?> GetByEmailAync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByIdAsync(Guid customerId)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<Customer?> GetByPhoneNumber(string phoneNumber)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsExistByIdAsync(Guid customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }


    }
}
