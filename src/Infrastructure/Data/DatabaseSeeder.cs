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
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@test.com";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "AdminPassword123!";
            var adminPhone = Environment.GetEnvironmentVariable("ADMIN_PHONE") ?? "09123456789";

            await EnsureUserInRoleAsync(userManager, adminEmail, adminPassword, adminPhone, "Admin", "Admin", "User");

            var userEmail = Environment.GetEnvironmentVariable("SUPPORT_EMAIL") ?? "user@test.com";
            var userPassword = Environment.GetEnvironmentVariable("SUPPORT_PASSWORD") ?? "UserPassword123!";
            var userPhone = Environment.GetEnvironmentVariable("SUPPORT_PHONE") ?? "09987654321";

            await EnsureUserInRoleAsync(userManager, userEmail, userPassword, userPhone, "User", "Regular", "User");
        }

        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string phoneNumber,
            string roleName,
            string firstName,
            string lastName)
        {
            var normalizedEmail = email.ToLowerInvariant();
            var existingUsers = await userManager.Users
                .Where(u => u.Email != null && u.Email.ToLower() == normalizedEmail)
                .ToListAsync();

            var user = existingUsers.FirstOrDefault();

            if (existingUsers.Count > 1)
            {
                foreach (var duplicate in existingUsers.Skip(1))
                {
                    await userManager.DeleteAsync(duplicate);
                }
            }

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
