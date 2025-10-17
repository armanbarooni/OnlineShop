using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;

namespace OnlineShop.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development"); // Use Development to load appsettings properly

            builder.ConfigureServices(services =>
            {
                // Remove the existing ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}");
                });

                // Replace ISmsService with TestSmsService
                var smsServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ISmsService));

                if (smsServiceDescriptor != null)
                {
                    services.Remove(smsServiceDescriptor);
                }

                services.AddScoped<ISmsService, TestSmsService>();

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<OnlineShop.Domain.Entities.ApplicationUser>>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed roles and admin user
                    SeedTestData(roleManager, userManager).GetAwaiter().GetResult();
                }
            });
        }

        private static async Task SeedTestData(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Create roles
            string[] roles = { "Admin", "User", "Manager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Create admin user with phone number
            var adminPhoneNumber = "09123456789";
            var adminUser = await userManager.FindByNameAsync(adminPhoneNumber);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminPhoneNumber,
                    PhoneNumber = adminPhoneNumber,
                    Email = "admin@test.com",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}

