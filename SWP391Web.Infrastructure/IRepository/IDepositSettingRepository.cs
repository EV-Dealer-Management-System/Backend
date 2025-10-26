using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDepositSettingRepository : IRepository<DepositSetting>
    {
        Task<DepositSetting?> GetByDealerIdAsync(Guid dealerId, CancellationToken ct);
        Task<DepositSetting?> GetByUserIdAsync(string userId, CancellationToken ct);
        Task<DepositSetting?> GetByDefaultAsync(CancellationToken ct);
    }
}
