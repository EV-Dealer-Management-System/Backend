using SWP391Web.Domain.Enums;

namespace SWP391Web.Domain.Entities
{
    public class ElectricVehicle
    {
        public Guid Id { get; set; }
        public Guid VersionId { get; set; }
        public Guid ColorId { get; set; }
        public Guid? DealerId { get; set; }
        public string VIN { get; set; } = null!;
        public ElectricVehicleStatus Status { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime? ImportDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string CurrentLocation { get; set; } = null!;
        public DateTime? DeliveryDate { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime? DealerReceivedDate { get; set; }
        public string? ImageUrl { get; set; }

        public ElectricVehicleVersion Version { get; set; } = null!;
        public ElectricVehicleColor Color { get; set; } = null!;
        public Dealer? Dealer { get; set; }
    }
}
