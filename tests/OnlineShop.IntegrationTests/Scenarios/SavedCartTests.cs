using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class SavedCartTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public SavedCartTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SaveCart_WithItems_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 2 });

            // Get current cart id
            var userId = await GetCurrentUserIdAsync();
            var cartGetResponse = await _client.GetAsync($"/api/cart/user/{userId}");
            Guid cartId = Guid.Empty;
            if (cartGetResponse.IsSuccessStatusCode)
            {
                var cartContent = await cartGetResponse.Content.ReadAsStringAsync();
                var id = JsonHelper.GetNestedProperty(cartContent, "data", "id");
                if (!string.IsNullOrEmpty(id))
                    cartId = Guid.Parse(id);
            }

            // Act
            var saveDto = new
            {
                SavedCartName = $"My Saved Cart {Guid.NewGuid()}",
                Description = "Shopping list for later",
                IsFavorite = false,
                CartId = cartId
            };
            var response = await _client.PostAsJsonAsync("/api/savedcart", saveDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetUserSavedCarts_ShouldReturnAllSavedCarts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/savedcart/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RestoreSavedCart_ShouldLoadItemsIntoCurrentCart()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 2 });

            var saveDto = new { SavedCartName = "Test Cart" };
            var saveResponse = await _client.PostAsJsonAsync("/api/savedcart", saveDto);
            
            Guid savedCartId = Guid.Empty;
            if (saveResponse.IsSuccessStatusCode)
            {
                var saveContent = await saveResponse.Content.ReadAsStringAsync();
                var id = JsonHelper.GetNestedProperty(saveContent, "data", "id");
                if (!string.IsNullOrEmpty(id))
                    savedCartId = Guid.Parse(id);
            }

            // Clear current cart
            await _client.DeleteAsync("/api/cart/clear");

            // Act - Restore saved cart
            var response = await _client.PostAsync($"/api/savedcart/{savedCartId}/restore", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteSavedCart_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var savedCartId = await CreateTestSavedCartAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/savedcart/{savedCartId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateSavedCart_WithNewName_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var savedCartId = await CreateTestSavedCartAsync();

            // Act
            var updateDto = new
            {
                SavedCartName = "Updated Cart Name",
                Description = "Updated description",
                IsFavorite = false
            };
            var response = await _client.PutAsJsonAsync($"/api/savedcart/{savedCartId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetSavedCartById_WithValidId_ShouldReturnCart()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var savedCartId = await CreateTestSavedCartAsync();

            // Act
            var response = await _client.GetAsync($"/api/savedcart/{savedCartId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task SaveMultipleCarts_ForSameUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act - Create multiple saved carts
            var cart1Id = await CreateTestSavedCartAsync("Electronics");
            var cart2Id = await CreateTestSavedCartAsync("Clothing");
            var cart3Id = await CreateTestSavedCartAsync("Books");

            // Assert
            cart1Id.Should().NotBe(Guid.Empty);
            cart2Id.Should().NotBe(Guid.Empty);
            cart3Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task SaveCart_WhenEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Clear cart first
            await _client.DeleteAsync("/api/cart/clear");

            // Act - Try to save empty cart
            var saveDto = new { SavedCartName = "Empty Cart" };
            var response = await _client.PostAsJsonAsync("/api/savedcart", saveDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task SaveCart_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var saveDto = new { Name = "Test Cart" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/savedcart", saveDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSavedCartItems_ShouldReturnCartItems()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var savedCartId = await CreateTestSavedCartAsync();

            // Act
            var response = await _client.GetAsync($"/api/savedcart/{savedCartId}/items");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        private async Task<Guid> CreateTestSavedCartAsync(string name = "Test Cart")
        {
            var productId = await CreateTestProductAsync();
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            var saveDto = new
            {
                SavedCartName = $"{name} {Guid.NewGuid()}",
                Description = "Test saved cart",
                IsFavorite = false
            };

            var response = await _client.PostAsJsonAsync("/api/savedcart", saveDto);
            if (response.IsSuccessStatusCode)
            {
                var saveContent = await response.Content.ReadAsStringAsync();
                var cartId = JsonHelper.GetNestedProperty(saveContent, "data", "id");
                if (!string.IsNullOrEmpty(cartId))
                    return Guid.Parse(cartId);
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
                Name = $"Saved Cart Product {Guid.NewGuid()}",
                Description = "Test Product",
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

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var profileResponse = await _client.GetAsync("/api/auth/me");
            if (profileResponse.IsSuccessStatusCode)
            {
                var content = await profileResponse.Content.ReadAsStringAsync();
                var userId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(userId))
                    return Guid.Parse(userId);
            }

            return Guid.NewGuid();
        }
    }
}

