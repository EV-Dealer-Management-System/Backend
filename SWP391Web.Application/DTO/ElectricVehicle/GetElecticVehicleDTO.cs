using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SWP391Web.Application.DTO.BookingEVDetail;
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
        public Guid WarehouseId { get; set; }
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public string VIN { get; set; }
        public StatusVehicle Status { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public decimal CostPrice { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class ViewVersionName
    {
        public Guid VersionId { get; set; }
        public string? VersionName { get; set; }
        public Guid ModelId { get; set; }
        public string? ModelName { get; set; }
    }

    public class ViewColorName
    {
        public Guid ColorId{ get; set; }
        public string? ColorName{ get; set; }
    }
}
