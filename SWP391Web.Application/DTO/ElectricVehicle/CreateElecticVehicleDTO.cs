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
        public Guid WarehouseId { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public string VIN { get; set; }
        public StatusVehicle Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public decimal CostPrice { get; set; }
        public string ImageUrl { get; set; }
    }
}
