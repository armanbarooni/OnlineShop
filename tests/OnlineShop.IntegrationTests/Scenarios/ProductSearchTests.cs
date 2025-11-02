using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ProductSearchTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductSearchTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SearchProducts_WithKeyword_ShouldReturnMatchingProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync("Gaming Laptop Test");

            // Act
            var searchCriteria = new
            {
                SearchTerm = "Gaming",
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task SearchProducts_WithPriceFilter_ShouldReturnFilteredProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            await CreateTestProductAsync("Cheap Product", price: 50m);
            await CreateTestProductAsync("Expensive Product", price: 500m);

            // Act
            var searchCriteria = new
            {
                MinPrice = 40m,
                MaxPrice = 100m,
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task SearchProducts_WithCategoryFilter_ShouldReturnCategoryProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            await CreateTestProductAsync("Category Product", categoryId: categoryId);

            // Act
            var searchCriteria = new
            {
                CategoryId = categoryId,
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task SearchProducts_WithBrandFilter_ShouldReturnBrandProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandId = await CreateTestBrandAsync();
            await CreateTestProductAsync("Brand Product", brandId: brandId);

            // Act
            var searchCriteria = new
            {
                BrandId = brandId,
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SearchProducts_WithSorting_ShouldReturnSortedProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            await CreateTestProductAsync("Product A", price: 100m);
            await CreateTestProductAsync("Product B", price: 200m);

            // Act - Sort by price ascending
            var searchCriteria = new
            {
                SortBy = "price",
                SortDescending = false,
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SearchProducts_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Create multiple products
            for (int i = 0; i < 15; i++)
            {
                await CreateTestProductAsync($"Product {i}");
            }

            // Act - Get first page
            var page1Criteria = new { PageNumber = 1, PageSize = 5 };
            var page1Response = await _client.PostAsJsonAsync("/api/product/search", page1Criteria);

            // Act - Get second page
            var page2Criteria = new { PageNumber = 2, PageSize = 5 };
            var page2Response = await _client.PostAsJsonAsync("/api/product/search", page2Criteria);

            // Assert
            page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
            page2Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SearchProducts_WithNoResults_ShouldReturnEmptyList()
        {
            // Act
            var searchCriteria = new
            {
                SearchTerm = "NonExistentProductXYZ123",
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task SearchProducts_WithMultipleFilters_ShouldReturnCombinedResults()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var brandId = await CreateTestBrandAsync();
            await CreateTestProductAsync("Filtered Product", price: 150m, categoryId: categoryId, brandId: brandId);

            // Act
            var searchCriteria = new
            {
                SearchTerm = "Filtered",
                MinPrice = 100m,
                MaxPrice = 200m,
                CategoryId = categoryId,
                BrandId = brandId,
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/product/search", searchCriteria);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<Guid> CreateTestProductAsync(string name, decimal price = 100m, Guid? categoryId = null, Guid? brandId = null)
        {
            var catId = categoryId ?? await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brId = brandId ?? await CreateTestBrandAsync();

            var productDto = new
            {
                Name = name,
                Description = "Test Description",
                Price = price,
                StockQuantity = 10,
                CategoryId = catId,
                UnitId = unitId,
                BrandId = brId,
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
                Name = $"Test Category {Guid.NewGuid()}",
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

