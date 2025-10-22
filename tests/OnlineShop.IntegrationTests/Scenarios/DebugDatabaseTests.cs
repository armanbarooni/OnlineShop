using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class DebugDatabaseTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DebugDatabaseTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task DebugDatabaseSeeding()
        {
            // Create a scope to check if admin user exists
            var serviceProvider = _factory.Services;
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                
                // Check if admin user exists
                var adminUser = await userManager.FindByEmailAsync("admin@test.com");
                
                Console.WriteLine($"Admin user found: {adminUser != null}");
                if (adminUser != null)
                {
                    Console.WriteLine($"Admin user ID: {adminUser.Id}");
                    Console.WriteLine($"Admin user email: {adminUser.Email}");
                    Console.WriteLine($"Admin user username: {adminUser.UserName}");
                    Console.WriteLine($"Admin user phone: {adminUser.PhoneNumber}");
                    
                    // Check roles
                    var roles = await userManager.GetRolesAsync(adminUser);
                    Console.WriteLine($"Admin user roles: {string.Join(", ", roles)}");
                }
                else
                {
                    Console.WriteLine("Admin user not found!");
                    
                    // List all users
                    var allUsers = userManager.Users.ToList();
                    Console.WriteLine($"Total users in database: {allUsers.Count}");
                    foreach (var user in allUsers)
                    {
                        Console.WriteLine($"User: {user.Email} | {user.UserName} | {user.PhoneNumber}");
                    }
                }
                
                // This should pass if admin user exists
                adminUser.Should().NotBeNull("Admin user should be seeded in test database");
            }
        }
    }
}
