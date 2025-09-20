using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.VehicleColorDTO
{
    public class CreateVehicleColorDTO
    {
        public string? ColorName { get; set; }
        public decimal ExtraPrice { get; set; }
    }
}
