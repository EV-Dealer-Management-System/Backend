using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid CustomerOrderId { get; set; }
        public string Provider { get; set; } = null!;
        public string OrderRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public CustomerOrder CustomerOrder { get; set; } = null!;
    }
}
