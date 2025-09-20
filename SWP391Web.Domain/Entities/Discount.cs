using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Discount
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        [Range(0, 100)]
        public DiscountType DiscountType { get; set; }
        public decimal? Percentage { get; set; }
        public int? FixedAmount { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Boolean IsActive { get; set; } = true;
    }
}
