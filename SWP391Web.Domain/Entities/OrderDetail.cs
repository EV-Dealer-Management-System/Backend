using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid CustomerOrderId { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CustomerOrder CustomerOrder { get; set; } = null!;
        public ElectricVehicle ElectricVehicle { get; set; } = null!;
    }
}
