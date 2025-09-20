using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Vehicle
{
    public class UpdateVehicleDTO
    {
        public string? Name { get; set; }
        public string? Model { get; set; }
        public string? Version { get; set; }
        public decimal? BasePrice { get; set; }
        public bool? IsApprovedByManager { get; set; }
    }
}
