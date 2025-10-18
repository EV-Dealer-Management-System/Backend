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
    public class DealerMemberRepository : Repository<DealerMember>, IDealerMemberRepository
    {
        private readonly ApplicationDbContext _context;
        public DealerMemberRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public Task<bool> IsActiveDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct)
        {
            return _context.DealerMembers
                .AnyAsync(dl => dl.DealerId == dealerId && dl.ApplicationUser.Email == email && dl.IsActive == true, ct);
        }

        public async Task<bool> IsExistDealerMemberByEmailAsync(Guid dealerId, string email, CancellationToken ct)
        {
            return await _context.DealerMembers
                .AnyAsync(dl => dl.DealerId == dealerId && dl.ApplicationUser.Email == email, ct);
        }
    }
}
