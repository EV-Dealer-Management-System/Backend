using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.ElectricVehicleColor
{
    public class CreateElectricVehicleColorDTO
    {
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public decimal ExtraCost { get; set; }
    }

    public class UpdateElectricVehicleColor
    {
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public decimal? ExtraCost { get; set; }
    }
}
