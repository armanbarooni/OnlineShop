using System.Net;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class EdgeCasesTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EdgeCasesTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetNonExistentProduct_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/product/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetNonExistentCategory_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/productcategory/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetNonExistentUnit_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/unit/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

