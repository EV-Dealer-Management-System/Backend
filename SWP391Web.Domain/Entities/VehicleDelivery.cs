using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class VehicleDelivery
    {
        public Guid Id { get; set; }
        public Guid BookingEVId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DeliveryStatus Status { get; set; } // enum: Preparing, Packing, InTransit, Arrived, Confirmed, Accident
        public DateTime? UpdateAt { get; set; } = DateTime.UtcNow;
    }
}
