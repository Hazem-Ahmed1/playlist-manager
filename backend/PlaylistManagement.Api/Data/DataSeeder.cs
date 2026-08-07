using Microsoft.AspNetCore.Identity;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Data
{
    /// <summary>
    /// Seeds Identity roles and development-only demo accounts (one Admin,
    /// one regular User). Runtime seeding (rather than EF's HasData) is
    /// required here because IdentityUser ids and password hashes must go
    /// through UserManager, not be hardcoded into a migration.
    /// </summary>
    public static class DataSeeder
    {
        private const string AdminEmail = "admin@playlist.local";
        private const string AdminPassword = "Admin@12345";

        private const string DemoUserEmail = "user@playlist.local";
        private const string DemoUserPassword = "User@12345";

        public static async Task SeedAsync(IServiceProvider services, IWebHostEnvironment environment)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in new[] { Roles.Admin, Roles.User })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seeding known credentials is only safe for local development,
            // so none of this runs outside that environment.
            if (!environment.IsDevelopment())
            {
                return;
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedUserAsync(userManager, AdminEmail, AdminPassword, "System", "Admin", Roles.Admin);
            await SeedUserAsync(userManager, DemoUserEmail, DemoUserPassword, "Demo", "User", Roles.User);
        }

        private static async Task SeedUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string firstName,
            string lastName,
            string role)
        {
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
