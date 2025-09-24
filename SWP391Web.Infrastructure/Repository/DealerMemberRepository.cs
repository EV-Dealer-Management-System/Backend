using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Constants;
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
    public class DealerMemberRepository : Repository<DealerMember>, IDealerMemberRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerMemberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DealerMember?> GetManagerAsync(Guid dealerId, CancellationToken ct)
        {
            var normalizedRole = StaticUserRole.DealerManager.ToUpperInvariant();
            var roleId = await _context.Roles
                .Where(r => r.NormalizedName == normalizedRole)
                .Select(r => r.Id).FirstOrDefaultAsync(ct);

            if (roleId is null)
                throw new ArgumentNullException("Role ID is null");

            var manager = await _context.Set<DealerMember>()
                .Where(dm => dm.DealerId == dealerId)
                .Where(dm => _context.Set<IdentityUserRole<string>>()
                .Any(ur => ur.UserId == dm.UserId && ur.RoleId == roleId))
                .OrderBy(dm => dm.CreatedAt)
                .Include(dm => dm.User)
                .FirstOrDefaultAsync();

            return manager;

        }
    }
}
