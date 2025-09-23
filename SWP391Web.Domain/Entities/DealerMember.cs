using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class DealerMember
    {
        public Guid DealerMemberId { get; set; }
        public Guid DealerId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        private readonly List<ApplicationUser> _users = new();
        public IReadOnlyCollection<ApplicationUser> Users => _users.AsReadOnly();
        public Dealer Dealer { get; set; } = null!;
    }
}
