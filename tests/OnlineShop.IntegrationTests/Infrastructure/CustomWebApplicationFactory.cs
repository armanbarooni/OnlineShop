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
                // Use a unique database name for each factory instance
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
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
                    // Use a permissive policy for tests
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .RequireAssertion(_ => true)
                        .Build();
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
            var adminEmail = "admin@test.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
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

