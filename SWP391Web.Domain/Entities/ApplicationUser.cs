using Microsoft.AspNetCore.Identity;

namespace SWP391Web.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Sex { get; set; }
        public DateOnly? DateOfBirth { get; set; }

    }
}
