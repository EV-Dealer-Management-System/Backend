using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    internal class VehicleDeliveryProcess
    {
        public Guid Id { get; set; }
        public Guid BookingEVId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DeliveryStatus Status { get; set; } // enum: Preparing, Packing, InTransit, Arrived, Confirmed, Accident
        public DateTime? UpdateAt { get; set; }

    }
}
