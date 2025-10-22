using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class TestAuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public TestAuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task TestAuthentication_ShouldWork()
        {
            // Test a simple endpoint that requires authentication
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
            var response = await _client.GetAsync("/api/useraddress");
            
            Console.WriteLine($"Status: {response.StatusCode}");
            
            // With test authentication, this should return 200 (not 401)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
