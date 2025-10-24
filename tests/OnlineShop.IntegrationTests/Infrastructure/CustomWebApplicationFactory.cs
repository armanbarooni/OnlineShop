using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authentication;

namespace OnlineShop.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private bool _seeded = false;
        private readonly object _lock = new object();
        private readonly string _databaseName;

        public CustomWebApplicationFactory()
        {
            _databaseName = $"TestDatabase_{Guid.NewGuid()}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development"); // Use Development to load appsettings with JWT config

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
                // Use a shared database name for all tests to maintain seed data
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDb_SharedForTests");
                });

                // Replace ISmsService with TestSmsService
                var smsServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ISmsService));

                if (smsServiceDescriptor != null)
                {
                    services.Remove(smsServiceDescriptor);
                }

                services.AddScoped<ISmsService, TestSmsService>();

                // Remove all JWT Bearer authentication registrations
                var jwtDescriptors = services.Where(d => 
                    d.ServiceType.FullName?.Contains("JwtBearer") == true ||
                    d.ServiceType.FullName?.Contains("Authentication") == true)
                    .ToList();
                
                foreach (var jwtDescriptor in jwtDescriptors)
                {
                    services.Remove(jwtDescriptor);
                }

                // Add test authentication as the default scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
                
                // Configure authorization for tests
                services.AddAuthorization(options =>
                {
                    // Remove the permissive default policy to allow proper role-based authorization
                    // The default policy will now require authentication and proper roles
                });
            });
        }

        public override IServiceProvider Services
        {
            get
            {
                var services = base.Services;
                
                // Seed database on first access
                if (!_seeded)
                {
                    lock (_lock)
                    {
                        if (!_seeded)
                        {
                            using (var scope = services.CreateScope())
                            {
                                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                                db.Database.EnsureCreated();
                                SeedTestData(roleManager, userManager).GetAwaiter().GetResult();
                            }
                            _seeded = true;
                        }
                    }
                }
                
                return services;
            }
        }

        public async Task SeedDatabaseAsync()
        {
            using (var scope = Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<OnlineShop.Domain.Entities.ApplicationUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed roles and admin user
                await SeedTestData(roleManager, userManager);
            }
        }

        private static async Task SeedTestData(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
        {
            Console.WriteLine("[CustomWebApplicationFactory] Starting test data seeding...");
            
            // Create roles
            string[] roles = { "Admin", "User", "Manager" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    Console.WriteLine($"[CustomWebApplicationFactory] Role '{roleName}' created: {roleResult.Succeeded}");
                }
                else
                {
                    Console.WriteLine($"[CustomWebApplicationFactory] Role '{roleName}' already exists");
                }
            }

            // Create admin user with phone number
            var adminPhoneNumber = "09123456789";
            var adminEmail = "admin@test.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                Console.WriteLine($"[CustomWebApplicationFactory] Creating admin user with phone: {adminPhoneNumber}");
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail, // Use email as username for easier login
                    PhoneNumber = adminPhoneNumber,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"[CustomWebApplicationFactory] Admin user created successfully. UserID: {adminUser.Id}");
                }
                else
                {
                    Console.WriteLine($"[CustomWebApplicationFactory] Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"[CustomWebApplicationFactory] Admin user already exists. UserID: {adminUser.Id}");
                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"[CustomWebApplicationFactory] Added Admin role to existing user");
                }
            }
            
            // Also ensure admin can be found by email
            var adminByEmail = await userManager.FindByEmailAsync("admin@test.com");
            if (adminByEmail == null)
            {
                Console.WriteLine($"[CustomWebApplicationFactory] WARNING: Admin user not found by email!");
            }
            else
            {
                Console.WriteLine($"[CustomWebApplicationFactory] Admin user found by email: {adminByEmail.Email}");
            }

            // Create regular user with phone number
            var userPhoneNumber = "09987654321";
            var userEmail = "user@test.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);
            
            if (regularUser == null)
            {
                regularUser = new ApplicationUser
                {
                    UserName = userEmail, // Use email as username for easier login
                    PhoneNumber = userPhoneNumber,
                    Email = userEmail,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    FirstName = "Regular",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(regularUser, "UserPassword123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
            else
            {
                // Ensure user has User role
                if (!await userManager.IsInRoleAsync(regularUser, "User"))
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }
    }
}

