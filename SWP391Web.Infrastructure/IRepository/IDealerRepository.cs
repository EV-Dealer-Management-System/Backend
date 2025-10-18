using Microsoft.AspNetCore.Identity;
using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerRepository : IRepository<Dealer>
    {
        Task<Dealer?> GetByIdAsync(Guid dealerId, CancellationToken ct);
        Task<Dealer?> GetManagerByUserIdAsync(string userId, CancellationToken ct);
        Task<bool> IsExistByNameAsync(string name, CancellationToken ct);
        Task<bool> IsExistByIdAsync(Guid id, CancellationToken ct);
        Task<ApplicationUser?> GetManagerByDealerId(Guid dealerId, CancellationToken ct);
        Task<Dealer?> GetDealerByUserIdAsync(string userId, CancellationToken ct);
        Task<Dealer?> GetDealerByManagerIdAsync(string managerId, CancellationToken ct);
    }
}
