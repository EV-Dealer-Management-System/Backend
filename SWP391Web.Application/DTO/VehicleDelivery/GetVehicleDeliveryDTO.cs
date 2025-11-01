using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.VehicleDelivery
{
    public class GetVehicleDeliveryDTO
    {
        public Guid Id { get; set; }
        public Guid BookingEVId { get; set; }
        public DeliveryStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdateAt { get; set; }
        public List<GetVehicleDTO> Vehicles { get; set; } = new List<GetVehicleDTO>();
        public class GetVehicleDTO
        {
            public Guid Id { get; set; }
            public string VIN { get; set; } = null!;
            public ElectricVehicleStatus Status { get; set; }
        }
    }
}
