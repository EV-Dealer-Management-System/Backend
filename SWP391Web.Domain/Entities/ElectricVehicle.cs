using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicle
    {
        public Guid? ElectricVehicleId { get; set; }
        public Guid? VersionId { get; set; }
        public Guid? ColorId { get; set; }
        public Guid? DealerId { get; set; }
        public string VIN { get; set; }
        public string Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime? DealerReceivedDate { get; set; }
        public string ImageUrl { get; set; }
    }
}
