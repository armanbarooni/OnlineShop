using System.Net;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ApiEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ApiEndpointTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/api/product")]
        [InlineData("/api/productcategory")]
        [InlineData("/api/unit")]
        [InlineData("/api/brand")]
        [InlineData("/api/material")]
        [InlineData("/api/season")]
        public async Task PublicEndpoints_ShouldBeAccessible(string endpoint)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("/api/cart")]
        [InlineData("/api/userorder")]
        [InlineData("/api/wishlist")]
        [InlineData("/api/userprofile")]
        public async Task ProtectedEndpoints_WithoutAuth_ShouldReturnUnauthorized(string endpoint)
        {
            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Theory]
        [InlineData("/api/product/{0}")]
        [InlineData("/api/productcategory/{0}")]
        [InlineData("/api/unit/{0}")]
        [InlineData("/api/brand/{0}")]
        public async Task GetByIdEndpoints_WithInvalidId_ShouldReturnNotFound(string endpointTemplate)
        {
            // Arrange
            var endpoint = string.Format(endpointTemplate, Guid.NewGuid());

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

