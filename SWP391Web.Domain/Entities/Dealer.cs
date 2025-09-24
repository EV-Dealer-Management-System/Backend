using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Dealer
    {
        public Guid DealerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        private readonly List<DealerMember> _dealerMembers = new();
        public IReadOnlyCollection<DealerMember> DealerMembers => _dealerMembers.AsReadOnly();
    }
}
