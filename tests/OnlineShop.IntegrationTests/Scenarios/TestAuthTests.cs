using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class TestAuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public TestAuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task TestAuthentication_ShouldWork()
        {
            // Get a real JWT token from AuthHelper
            var token = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            token.Should().NotBeNullOrEmpty("Admin token should be retrieved successfully");
            
            // Test a simple endpoint that requires authentication
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _client.GetAsync("/api/useraddress");
            
            Console.WriteLine($"Status: {response.StatusCode}");
            
            // With real JWT token, this should return 200 (not 401)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
