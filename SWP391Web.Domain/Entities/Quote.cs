using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Quote
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; } 
        public Guid DealerId { get; set; } 
        public Guid PromotionId { get; set; }
        public string CreatedById { get; set; } = null!; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public QuoteStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
    }
}
