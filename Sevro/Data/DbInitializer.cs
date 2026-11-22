using Microsoft.AspNetCore.Identity;
using Sevro.Models;

namespace Sevro.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Core Roles (Admin, Customer, Provider)
            string[] roles = { "Admin", "Customer", "Provider" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // 2. Seed Only the Single Initial Admin Account
            const string adminEmail = "admin@sevro.local";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Sevro Admin",
                    PhoneNumber = "9876543210",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // 3. Seed 30 Categories & Full Affordable INR Service Catalog (Idempotent)
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await CatalogSeeder.SeedAsync(dbContext);
        }
    }
}
