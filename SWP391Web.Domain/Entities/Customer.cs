using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public required string UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
