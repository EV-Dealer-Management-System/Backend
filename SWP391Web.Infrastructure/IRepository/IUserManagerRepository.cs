using Microsoft.AspNetCore.Identity;
using SWP391Web.Domain.Entities;

namespace SWP391Web.Infrastructure.IRepository
{
    public interface IUserManagerRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<bool> IsEmailExist(string email);
        Task<bool> IsPhoneNumber(string phoneNumber);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> AccessFailedAsync(ApplicationUser user);
        Task<IList<string>> GetRoleAsync(ApplicationUser user);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
        Task<IdentityResult> SetPassword(ApplicationUser user, string newPassword);
    }
}
