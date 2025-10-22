using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class DebugTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DebugTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task TestAuthentication_ShouldGetToken()
        {
            // Test direct login first
            Console.WriteLine("=== Testing Direct Login ===");
            var loginDto = new 
            { 
                Email = "admin@test.com", 
                Password = "AdminPassword123!" 
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            Console.WriteLine($"Login Status: {loginResponse.StatusCode}");
            
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Login Response: {loginContent}");
            
            // Test our AuthHelper
            Console.WriteLine("\n=== Testing AuthHelper ===");
            var token = await AuthHelper.GetAdminTokenAsync(_client, _factory);

            Console.WriteLine($"=== AuthHelper Result ===");
            Console.WriteLine($"Token: {(string.IsNullOrEmpty(token) ? "NULL OR EMPTY" : $"EXTRACTED (length: {token.Length})")}");

            Assert.True(!string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task TestCategoryCreation_ShouldCreateCategory()
        {
            var categoryDto = new
            {
                Name = $"Debug Category {Guid.NewGuid()}",
                Description = "Debug test",
                MahakClientId = (long)Random.Shared.Next(1000, 9999),
                MahakId = Random.Shared.Next(1000, 9999)
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);

            Console.WriteLine($"Category Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Category Content: {content}");

                var id = JsonHelper.GetNestedProperty(content, "data", "id");
                Console.WriteLine($"Extracted ID: {id}");

                Assert.NotNull(id);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Category Error: {error}");
            }
        }

        [Fact]
        public async Task TestPublicEndpoint_ShouldWork()
        {
            var response = await _client.GetAsync("/api/productcategory");

            Console.WriteLine($"Public Endpoint Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Public Content Length: {content.Length}");
                Assert.Contains("isSuccess", content);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Public Error: {error}");
            }
        }
    }
}

