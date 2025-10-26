using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByIdAsync(Guid customerId);
        Task<bool> IsExistByIdAsync(Guid customerId);
        Task<Customer?> GetByEmailAync(string email);
        Task<Customer?> GetByPhoneNumber(string phoneNumber);
    }
}
