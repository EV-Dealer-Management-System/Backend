using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class CustomerFBAttachment
    {
        Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public string FileName { get; set; }
        public Guid CustomerFeedBackId { get; set; }
    }
}
