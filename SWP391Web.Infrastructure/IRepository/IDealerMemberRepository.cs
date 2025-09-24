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
        Task<DealerMember?> GetManagerAsync(Guid dealerId, CancellationToken ct);
    }
}
