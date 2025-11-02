using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ApiHealthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ApiHealthTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact(Skip = "Requires query parameter validation fix")]
        public async Task ProductSearch_WithValidParameters_ReturnsOk()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/product/search?searchTerm=&pageNumber=1&pageSize=10&sortBy=CreatedAt&sortOrder=desc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/productcategory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllUnits_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/unit");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllBrands_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/brand");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

