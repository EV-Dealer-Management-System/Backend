using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<Discount?> GetByIdAsync(Guid discoutnId);
        Task<Discount?> GetByCodeAsync(string code);
        Task<bool> IsExistByCode(string code);
        Task<bool> IsExistByCodeExceptId(string code, Guid exceptId);
    }
}
