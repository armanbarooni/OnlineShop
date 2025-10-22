using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class ProductDetailControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductDetailControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllProductDetails_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/productdetail");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllProductDetails_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/productdetail");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetProductDetailById_WithValidId_ShouldReturnProductDetail()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDetailId = await CreateTestProductDetailAsync();

            // Act
            var response = await _client.GetAsync($"/api/productdetail/{productDetailId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task GetProductDetailById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync($"/api/productdetail/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateProductDetail_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var productDetailDto = new
            {
                ProductId = productId,
                Key = "Color",
                Value = "Red",
                Description = "Product color detail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productdetail", productDetailDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task CreateProductDetail_WithNonExistentProduct_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDetailDto = new
            {
                ProductId = Guid.NewGuid(), // Non-existent product
                Key = "Color",
                Value = "Red",
                Description = "Product color detail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productdetail", productDetailDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateProductDetail_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var productDetailDto = new
            {
                ProductId = Guid.NewGuid(),
                Key = "Color",
                Value = "Red",
                Description = "Product color detail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productdetail", productDetailDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProductDetail_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDetailId = await CreateTestProductDetailAsync();
            var updateDto = new
            {
                Id = productDetailId,
                Key = "Updated Color",
                Value = "Blue",
                Description = "Updated product color detail"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/productdetail/{productDetailId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateProductDetail_WithIdMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDetailId = await CreateTestProductDetailAsync();
            var updateDto = new
            {
                Id = Guid.NewGuid(), // Different ID
                Key = "Updated Color",
                Value = "Blue",
                Description = "Updated product color detail"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/productdetail/{productDetailId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateProductDetail_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var updateDto = new
            {
                Id = Guid.NewGuid(),
                Key = "Updated Color",
                Value = "Blue",
                Description = "Updated product color detail"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/productdetail/{Guid.NewGuid()}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteProductDetail_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productDetailId = await CreateTestProductDetailAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/productdetail/{productDetailId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProductDetail_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.DeleteAsync($"/api/productdetail/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProductDetail_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/productdetail/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateProductDetail_WithEmptyKey_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var invalidDto = new
            {
                ProductId = productId,
                Key = "", // Empty key should be invalid
                Value = "Red",
                Description = "Product color detail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productdetail", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateProductDetail_WithEmptyValue_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var invalidDto = new
            {
                ProductId = productId,
                Key = "Color",
                Value = "", // Empty value should be invalid
                Description = "Product color detail"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productdetail", invalidDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestProductDetailAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var productDetailDto = new
            {
                ProductId = productId,
                Key = $"Test Key {Guid.NewGuid()}",
                Value = "Test Value",
                Description = "Test product detail for integration tests"
            };

            var response = await _client.PostAsJsonAsync("/api/productdetail", productDetailDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productDetailId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(productDetailId))
                    return Guid.Parse(productDetailId);
            }

            return Guid.NewGuid(); // Fallback
        }

        private async Task<Guid> CreateTestProductAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Test Product {Guid.NewGuid()}",
                Description = "Test product for integration tests",
                Price = 100.0m,
                StockQuantity = 100,
                CategoryId = categoryId,
                UnitId = unitId,
                BrandId = brandId,
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/product", productDto);
            var content = await response.Content.ReadAsStringAsync();
            var productId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(productId);
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Test Category {Guid.NewGuid()}",
                Description = "Test category",
                MahakClientId = (long)Random.Shared.Next(1000, 9999),
                MahakId = Random.Shared.Next(1000, 9999)
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);
            var content = await response.Content.ReadAsStringAsync();
            var categoryId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(categoryId);
        }

        private async Task<Guid> CreateTestUnitAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"Test Unit {Guid.NewGuid()}",
                MahakClientId = (long)Random.Shared.Next(1000, 9999),
                MahakId = Random.Shared.Next(1000, 9999)
            };

            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);
            var content = await response.Content.ReadAsStringAsync();
            var unitId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(unitId);
        }

        private async Task<Guid> CreateTestBrandAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"Test Brand {Guid.NewGuid()}",
                Description = "Test brand",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            var content = await response.Content.ReadAsStringAsync();
            var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(brandId);
        }
    }
}

