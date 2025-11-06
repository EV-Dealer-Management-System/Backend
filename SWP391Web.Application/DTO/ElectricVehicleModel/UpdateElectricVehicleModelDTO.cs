using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.ElectricVehicleModel
{
    public class UpdateElectricVehicleModelDTO
    {
        public string? ModelName { get; set; }
        public string? Description { get; set; }
        public StatusModel Status { get; set; }


    }
}
