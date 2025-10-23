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
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
        public decimal TotalPrice { get; set; }

        public CustomerOrder CustomerOrder { get; set; } = null!;
        public ElectricVehicle ElectricVehicle { get; set; } = null!;
    }
}
