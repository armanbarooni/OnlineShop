using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("DatabaseSeeder");

            string[] roles = { "Admin", "User", "Manager" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    EnsureSucceeded(roleResult, $"create role '{roleName}'");
                    logger?.LogInformation("Seeded role {RoleName}", roleName);
                }
            }

            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@test.com";
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "AdminPassword123!";
            var adminPhone = Environment.GetEnvironmentVariable("ADMIN_PHONE") ?? "09123456789";

            await EnsureUserInRoleAsync(userManager, logger, adminEmail, adminPassword, adminPhone, "Admin", "Admin", "User");

            var userEmail = Environment.GetEnvironmentVariable("SUPPORT_EMAIL") ?? "user@test.com";
            var userPassword = Environment.GetEnvironmentVariable("SUPPORT_PASSWORD") ?? "UserPassword123!";
            var userPhone = Environment.GetEnvironmentVariable("SUPPORT_PHONE") ?? "09987654321";

            await EnsureUserInRoleAsync(userManager, logger, userEmail, userPassword, userPhone, "User", "Regular", "User");
        }

        private static async Task EnsureUserInRoleAsync(
            UserManager<ApplicationUser> userManager,
            ILogger? logger,
            string email,
            string password,
            string phoneNumber,
            string roleName,
            string firstName,
            string lastName)
        {
            var normalizedEmail = email.ToLowerInvariant();
            var normalizedPhone = phoneNumber.Trim();
            var existingUsers = await userManager.Users
                .Where(u =>
                    (u.Email != null && u.Email.ToLower() == normalizedEmail) ||
                    (u.UserName != null && u.UserName.ToLower() == normalizedEmail) ||
                    (u.PhoneNumber != null && u.PhoneNumber == normalizedPhone))
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
                EnsureSucceeded(result, $"create seed user '{email}'");
                logger?.LogInformation("Seeded user {Email}", email);
            }
            else
            {
                var changed = false;

                if (!string.Equals(user.UserName, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.UserName = email;
                    changed = true;
                }

                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    changed = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    changed = true;
                }

                if (!string.Equals(user.PhoneNumber, normalizedPhone, StringComparison.Ordinal))
                {
                    user.PhoneNumber = normalizedPhone;
                    changed = true;
                }

                if (!user.PhoneNumberConfirmed)
                {
                    user.PhoneNumberConfirmed = true;
                    changed = true;
                }

                if (user.FirstName != firstName)
                {
                    user.FirstName = firstName;
                    changed = true;
                }

                if (user.LastName != lastName)
                {
                    user.LastName = lastName;
                    changed = true;
                }

                if (changed)
                {
                    var updateResult = await userManager.UpdateAsync(user);
                    EnsureSucceeded(updateResult, $"update seed user '{email}'");
                    logger?.LogInformation("Updated seed user {Email}", email);
                }
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                EnsureSucceeded(roleResult, $"add seed user '{email}' to role '{roleName}'");
                logger?.LogInformation("Assigned role {RoleName} to seed user {Email}", roleName, email);
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string operation)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Database seed failed during {operation}: {errors}");
        }
    }
}
