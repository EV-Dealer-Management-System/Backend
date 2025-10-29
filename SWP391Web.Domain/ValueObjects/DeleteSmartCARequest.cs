using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.ValueObjects
{
    public class DeleteSmartCARequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
