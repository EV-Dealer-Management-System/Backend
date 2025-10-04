using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicleColor
    {
        public Guid? ColorId { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public decimal ExtraCost { get; set; }
    }
}
