using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Warehouse
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public Guid? EVInventoryId { get; set; }
        public WarehouseType WarehouseType { get; set; }

        public ICollection<ElectricVehicle> ElectricVehicles { get; set; } = new List<ElectricVehicle>();
        public Dealer? Dealer { get; set; }
        public EVInventory? EVInventory { get; set; }
    }
}
