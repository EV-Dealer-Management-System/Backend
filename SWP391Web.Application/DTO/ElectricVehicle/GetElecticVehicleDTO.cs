using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.ElectricVehicle
{
    public class GetElecticVehicleDTO
    {
        public Guid Id { get; set; }
        public string VIN { get; set; }
        public StatusVehicle Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string CurrentLocation { get; set; }
        public decimal CostPrice { get; set; }
        public string ImageUrl { get; set; }
    }
}
