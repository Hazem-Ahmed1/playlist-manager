using Microsoft.AspNetCore.Identity;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.Api.Data
{
    /// <summary>
    /// Seeds Identity roles and a development-only admin account. Runtime
    /// seeding (rather than EF's HasData) is required here because
    /// IdentityUser ids and password hashes must go through UserManager,
    /// not be hardcoded into a migration.
    /// </summary>
    public static class DataSeeder
    {
        private const string AdminEmail = "admin@playlist.local";
        private const string AdminPassword = "Admin@12345";

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

            // Seeding a known admin credential is only safe for local
            // development, so it never runs outside that environment.
            if (!environment.IsDevelopment())
            {
                return;
            }

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync(AdminEmail) is not null)
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FirstName = "System",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
    }
}
