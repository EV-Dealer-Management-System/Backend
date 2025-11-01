using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.VehicleDelivery
{
    public class UpdateVehicleDeliveryDTO
    {
        public Guid Id { get; set; }
        public DeliveryStatus NewStatus { get; set; }
        public string? Description { get; set; }
    }
}
