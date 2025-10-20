﻿using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicle
    {
        public Guid Id { get; set; }
        public Guid ElectricVehicleTemplateId {  get; set; }
        public Guid WarehouseId { get; set; }
        public string VIN { get; set; } = null!;
        public StatusVehicle Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? DealerReceivedDate { get; set; }

        public Warehouse Warehouse { get; set; } = null!;
        public ElectricVehicleTemplate ElectricVehicleTemplate { get; set; } = null!;
    }
}
