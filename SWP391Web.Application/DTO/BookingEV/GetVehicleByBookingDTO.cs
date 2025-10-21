using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.DTO.EVTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.BookingEV
{
    public class GetVehicleByBookingDTO
    {
        public Guid ElectricVehicleId { get; set; }
        public string VIN { get; set; }
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public ViewWarehouse? Warehouse { get; set; }
    }
}
