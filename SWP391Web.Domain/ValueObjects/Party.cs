using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public record Party
    {
        public Guid PartyId { get; set; }
        public PartyType PartyType { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
