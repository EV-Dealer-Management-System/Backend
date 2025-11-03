using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class VehicleDeliveryDetail
    {
        public Guid Id { get; set; }
        public Guid VehicleDeliveryId { get; set; }
        public Guid ElectricVehicleId { get; set; }
        public DeliveryVehicleStatus Status { get; set; } // ví dụ: InTransit, Delivered, Damaged
        public string? Note { get; set; }

    }
}
