using Microsoft.AspNetCore.Identity;

namespace Pinewood.Api.Data
{
    public static class SeedData
    {
        public static async Task Initialize(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            await EnsureRoleExists(roleManager, "Admin");
            await EnsureRoleExists(roleManager, "Staff");
            await EnsureRoleExists(roleManager, "Customer");

            await EnsureUserExists(userManager, "generalcaw@gmail.com", "Admin");
        }

        private static async Task EnsureRoleExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                await roleManager.CreateAsync(role);
            }
        }

        private static async Task EnsureUserExists(UserManager<IdentityUser> userManager, string email, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "PineWood123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
