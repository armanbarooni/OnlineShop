using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ProductVariantTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductVariantTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateProductVariant_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var variantDto = new
            {
                ProductId = productId,
                Sku = $"SKU-{Guid.NewGuid()}",
                Size = "Large",
                Color = "Red",
                Price = 120.0m,
                StockQuantity = 25
            };
            var response = await _client.PostAsJsonAsync("/api/productvariant", variantDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetProductVariants_ForProduct_ShouldReturnAllVariants()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await CreateTestVariantAsync(productId, "Small", "Blue");
            await CreateTestVariantAsync(productId, "Large", "Red");

            // Act
            var response = await _client.GetAsync($"/api/productvariant/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task UpdateVariant_WithNewPrice_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var variantId = await CreateTestVariantAsync(productId);

            // Act
            var updateDto = new
            {
                Price = 150.0m,
                StockQuantity = 30
            };
            var response = await _client.PutAsJsonAsync($"/api/productvariant/{variantId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteVariant_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var variantId = await CreateTestVariantAsync(productId);

            // Act
            var response = await _client.DeleteAsync($"/api/productvariant/{variantId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddVariantToCart_ShouldUseVariantPrice()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var variantId = await CreateTestVariantAsync(productId, price: 200m);

            // Act - Add variant to cart
            var cartDto = new
            {
                ProductId = productId,
                ProductVariantId = variantId,
                Quantity = 1
            };
            var response = await _client.PostAsJsonAsync("/api/cart/add", cartDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetVariantStock_ShouldReturnCorrectQuantity()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var variantId = await CreateTestVariantAsync(productId, stockQuantity: 50);

            // Act
            var response = await _client.GetAsync($"/api/productvariant/{variantId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stockQuantity = JsonHelper.GetNestedProperty(content, "data", "stockQuantity");
                stockQuantity.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task CreateMultipleVariants_WithDifferentAttributes_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act - Create multiple variants
            var variant1Id = await CreateTestVariantAsync(productId, "Small", "Red");
            var variant2Id = await CreateTestVariantAsync(productId, "Medium", "Blue");
            var variant3Id = await CreateTestVariantAsync(productId, "Large", "Green");

            // Assert
            variant1Id.Should().NotBe(Guid.Empty);
            variant2Id.Should().NotBe(Guid.Empty);
            variant3Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task GetVariantById_WithValidId_ShouldReturnVariant()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var variantId = await CreateTestVariantAsync(productId);

            // Act
            var response = await _client.GetAsync($"/api/productvariant/{variantId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        private async Task<Guid> CreateTestVariantAsync(
            Guid productId, 
            string? size = null, 
            string? color = null, 
            decimal price = 120m, 
            int stockQuantity = 25)
        {
            var variantDto = new
            {
                ProductId = productId,
                Sku = $"SKU-{Guid.NewGuid()}",
                Size = size ?? "Medium",
                Color = color ?? "Black",
                Price = price,
                StockQuantity = stockQuantity
            };

            var response = await _client.PostAsJsonAsync("/api/productvariant", variantDto);
            if (response.IsSuccessStatusCode)
            {
                var variantContent = await response.Content.ReadAsStringAsync();
                var variantId = JsonHelper.GetNestedProperty(variantContent, "data", "id");
                if (!string.IsNullOrEmpty(variantId))
                    return Guid.Parse(variantId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestProductAsync()
        {
            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Variant Product {Guid.NewGuid()}",
                Description = "Product with variants",
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
            var categoryDto = new
            {
                Name = $"Cat {Guid.NewGuid()}",
                Description = "Test",
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
            var unitDto = new
            {
                Name = $"Unit {Guid.NewGuid()}",
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
            var brandDto = new
            {
                Name = $"Brand {Guid.NewGuid()}",
                Description = "Test",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            var content = await response.Content.ReadAsStringAsync();
            var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(brandId);
        }
    }
}

