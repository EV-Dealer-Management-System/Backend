using Microsoft.AspNetCore.Identity;

namespace SWP391Web.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Sex { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Dealer> ManagingDealers { get; set; } = new List<Dealer>();

        public ICollection<EContract> EContracts { get; set; } = new List<EContract>();
        public Quote Quote = null!;
        public ICollection<DealerMember> DealerMembers { get; set; } = new List<DealerMember>();
    }
}
