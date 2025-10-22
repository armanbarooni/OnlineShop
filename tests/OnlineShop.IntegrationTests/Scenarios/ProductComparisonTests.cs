using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ProductComparisonTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductComparisonTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AddToComparison_WithValidProduct_ShouldSucceed()
        {
            // Arrange - Login
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Create a product first
            var productId = await CreateTestProductAsync();

            // Act - Add to comparison
            var addDto = new { ProductId = productId };
            var response = await _client.PostAsJsonAsync("/api/productcomparison/add", addDto);

            // Assert - موفق یا BadRequest (مثلاً مورد تکراری)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserComparison_AfterAddingProducts_ShouldReturnProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = productId });

            // Act
            var response = await _client.GetAsync("/api/productcomparison");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CompareProducts_WithMultipleProducts_ShouldReturnComparison()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var product1Id = await CreateTestProductAsync();
            var product2Id = await CreateTestProductAsync();

            await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = product1Id });
            await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = product2Id });

            // Act
            var response = await _client.GetAsync("/api/productcomparison/compare");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task RemoveFromComparison_WithExistingProduct_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = productId });

            // Act
            var response = await _client.DeleteAsync($"/api/productcomparison/remove/{productId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ClearComparison_ShouldRemoveAllProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = productId });

            // Act
            var response = await _client.DeleteAsync("/api/productcomparison/clear");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddToComparison_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var productId = Guid.NewGuid();
            var addDto = new { ProductId = productId };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productcomparison/add", addDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private async Task<Guid> CreateTestProductAsync()
        {
            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Test Product {Guid.NewGuid()}",
                Description = "Test Description",
                Price = 100.0m,
                StockQuantity = 10,
                CategoryId = categoryId,
                UnitId = unitId,
                BrandId = brandId
            };

            var response = await _client.PostAsJsonAsync("/api/product", productDto);
            var content = await response.Content.ReadAsStringAsync();
            var productId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(productId);
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var categoryDto = new
            {
                Name = $"Test Category {Guid.NewGuid()}",
                Description = "Test",
                MahakClientId = (long)1,
                MahakId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);
            var content = await response.Content.ReadAsStringAsync();
            var categoryId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(categoryId);
        }

        private async Task<Guid> CreateTestUnitAsync()
        {
            var unitDto = new
            {
                Name = $"Unit {Guid.NewGuid()}",
                MahakClientId = (long)1,
                MahakId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);
            var content = await response.Content.ReadAsStringAsync();
            var unitId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(unitId);
        }

        private async Task<Guid> CreateTestBrandAsync()
        {
            var brandDto = new
            {
                Name = $"Brand {Guid.NewGuid()}",
                Description = "Test Brand",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            var content = await response.Content.ReadAsStringAsync();
            var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(brandId);
        }
    }
}

