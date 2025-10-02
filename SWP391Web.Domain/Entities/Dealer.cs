using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Dealer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DealerStatus DealerStatus { get; set; } = DealerStatus.Inactive;

        private readonly List<DealerMember> _dealerMembers = new();
        public IReadOnlyCollection<DealerMember> DealerMembers => _dealerMembers.AsReadOnly();
    }
}
