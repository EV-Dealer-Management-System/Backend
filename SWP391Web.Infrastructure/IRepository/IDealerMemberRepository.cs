using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IDealerMemberRepository : IRepository<DealerMember>
    {
        Task<bool> IsExistDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct);
        Task<bool> IsActiveDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct);
    }
}
