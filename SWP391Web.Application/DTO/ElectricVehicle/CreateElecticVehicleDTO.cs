using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.ElectricVehicle
{
    public class CreateElecticVehicleDTO
    {
        public Guid ElectricVehicleTemplateId { get; set; }
        public Guid WarehouseId { get; set; }
        public List<string> VINList { get; set; } = new();
        public StatusVehicle Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public decimal CostPrice { get; set; }
       
    }
}
