using Microsoft.AspNetCore.Identity;
using SWP391Web.Domain.Constants;

namespace SWP391Web.Infrastructure.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = {
                StaticUserRole.Admin,
                StaticUserRole.Customer
            };

            foreach (var role in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
