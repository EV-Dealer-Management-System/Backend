using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class DepositSetting
    {
        public Guid Id { get; set; }
        public string ManagerId { get; set; } = null!;
        public Guid? DealerId { get; set; }
        public decimal? MinDepositPercentage { get; set; }
        public decimal MaxDepositPercentage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser Manager { get; set; } = null!;
        public Dealer? Dealer { get; set; }
    }
}
