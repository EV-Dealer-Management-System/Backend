using SWP391Web.Application.DTO.ElectricVehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EVTemplate
{
    public class GetEVTemplateDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public string Description { get; set; }
        public List<string> ImgUrl { get; set; } = new();
    }
}
