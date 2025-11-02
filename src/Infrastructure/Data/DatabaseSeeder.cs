using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "User", "Manager" };

            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Seed admin user
            await SeedAdminUserAsync(userManager, dbContext);
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            var adminEmail = "admin@onlineshop.com";
            
            // Use direct query to avoid "Sequence contains more than one element" exception
            // that can occur with FindByEmailAsync in In-Memory databases
            ApplicationUser adminUser = null;
            try
            {
                adminUser = await userManager.FindByEmailAsync(adminEmail);
            }
            catch (InvalidOperationException)
            {
                // If FindByEmailAsync fails (duplicate emails in In-Memory), use direct query
                adminUser = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == adminEmail);
            }

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "09123456789",
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

