using SWP391Web.Domain.Enums;

namespace SWP391Web.Domain.Entities
{
    public class Dealer
    {
        public Guid Id { get; set; }
        public string? ManagerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string TaxNo { get; set; } = null!;
        public DealerStatus DealerStatus { get; set; } = DealerStatus.Inactive;

        public ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();
        public ApplicationUser? Manager { get; set; }
        public ICollection<ElectricVehicle> ElectricVehicles { get; set; } = new List<ElectricVehicle>();
    }
}
