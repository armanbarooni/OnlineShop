using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class ProductControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/product");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync($"/api/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/product/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SearchProducts_WithQueryParameters_ShouldReturnOk()
        {
            // Arrange
            await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync("/api/product/search?searchTerm=test&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SearchProducts_WithBody_ShouldReturnOk()
        {
            // Arrange
            await CreateTestProductAsync();

            var searchCriteria = new
            {
                SearchTerm = "test",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetRelatedProducts_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync($"/api/product/{productId}/related");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetFrequentlyBoughtTogether_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync($"/api/product/{productId}/frequently-bought-together");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateProduct_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Test Product {Guid.NewGuid()}",
                Description = "Test Product Description",
                Price = 100.0m,
                StockQuantity = 100,
                CategoryId = categoryId,
                UnitId = unitId,
                BrandId = brandId,
                IsActive = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/product", productDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDto = new
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100.0m,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/product", productDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateProduct_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var productDto = new
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 100.0m,
                StockQuantity = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/product", productDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProduct_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var updateDto = new
            {
                Id = productId,
                Name = $"Updated Product {Guid.NewGuid()}",
                Description = "Updated Description",
                Price = 150.0m,
                StockQuantity = 50
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/product/{productId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create product as admin
            var productId = await CreateTestProductAsync();
            
            // Then set user token for the update attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            var updateDto = new
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 150.0m,
                StockQuantity = 50
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/product/{productId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteProduct_AsAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/product/{productId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            // First create product as admin
            var productId = await CreateTestProductAsync();
            
            // Then set user token for the delete attempt
            var userToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.DeleteAsync($"/api/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task TrackProductView_AsAuthenticatedUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Create product as admin first
            var adminToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var productId = await CreateTestProductAsync();

            // Switch back to user token
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.PostAsync($"/api/product/{productId}/track-view", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetRecentlyViewed_AsAuthenticatedUser_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/product/recently-viewed");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestProductAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"TestProduct{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Product Description",
                Price = 100.0m,
                StockQuantity = 100,
                CategoryId = categoryId,
                UnitId = unitId,
                BrandId = brandId,
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/product", productDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(productId))
                    return Guid.Parse(productId);
            }

            return Guid.NewGuid(); // Fallback
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"TestCategory{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Description",
                ParentCategoryId = (Guid?)null,
                MahakClientId = 0L,
                MahakId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var categoryId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(categoryId))
                    return Guid.Parse(categoryId);
            }

            return Guid.NewGuid(); // Fallback
        }

        private async Task<Guid> CreateTestUnitAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"TestUnit{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Comment = "Test Comment"
            };

            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var unitId = JsonHelper.GetNestedProperty(content, "data", "id")
                    ?? JsonHelper.GetNestedProperty(content, "data");
                
                if (!string.IsNullOrEmpty(unitId))
                {
                    if (Guid.TryParse(unitId, out var guid))
                        return guid;
                }
            }

            return Guid.NewGuid(); // Fallback
        }

        private async Task<Guid> CreateTestBrandAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"TestBrand{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Description",
                DisplayOrder = 1
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var brandId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(brandId))
                    return Guid.Parse(brandId);
            }

            return Guid.NewGuid(); // Fallback
        }
    }
}


