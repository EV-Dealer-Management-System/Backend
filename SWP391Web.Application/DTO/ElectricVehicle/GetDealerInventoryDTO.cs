using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.ElectricVehicle
{
    public class GetDealerInventoryDTO
    {
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public int Quantity { get; set; }
    }
}
