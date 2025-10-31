using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public class DeleteSmartCARequest
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
    }
}
