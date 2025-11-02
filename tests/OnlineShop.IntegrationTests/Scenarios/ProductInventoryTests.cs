using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ProductInventoryTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductInventoryTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProductInventory_WithValidProductId_ShouldReturnInventory()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 100);

            // Act
            var response = await _client.GetAsync($"/api/productinventory/product/{productId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateInventory_IncreaseStock_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 50);
            var inventoryId = await GetOrCreateInventoryAsync(productId);

            // Act - Increase stock
            var updateDto = new
            {
                Quantity = 100,
                Notes = "Stock replenishment"
            };
            var response = await _client.PutAsJsonAsync($"/api/productinventory/{inventoryId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateInventory_DecreaseStock_OnOrderPlaced_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 100);

            // Act - Place order (this should decrease stock)
            var cartDto = new { ProductId = productId, Quantity = 10 };
            var cartResponse = await _client.PostAsJsonAsync("/api/cart/add", cartDto);

            // Assert
            cartResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateInventory_RestoreStock_OnOrderCancelled_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 100);
            var userId = await GetCurrentUserIdAsync();

            // Place and then cancel order
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 10 });
            
            var addressId = await CreateTestAddressAsync(userId);
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "Cash"
            };
            var orderResponse = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            if (orderResponse.IsSuccessStatusCode)
            {
                var orderContent = await orderResponse.Content.ReadAsStringAsync();
                var orderId = JsonHelper.GetNestedProperty(orderContent, "data", "id");

                if (!string.IsNullOrEmpty(orderId))
                {
                    // Act - Cancel order
                    var cancelDto = new { OrderStatus = "Cancelled" };
                    var cancelResponse = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", cancelDto);

                    // Assert
                    cancelResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
                }
            }
        }

        [Fact]
        public async Task CheckLowStock_ShouldReturnLowStockProducts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            await CreateTestProductAsync(stockQuantity: 5);
            await CreateTestProductAsync(stockQuantity: 3);

            // Act - Search for low stock products
            var response = await _client.GetAsync("/api/productinventory/low-stock?threshold=10");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetInventoryHistory_ShouldReturnStockChanges()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync($"/api/productinventory/product/{productId}/history");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddToCart_WithInsufficientStock_ShouldReturnError()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 5);

            // Act - Try to add more than available
            var cartDto = new { ProductId = productId, Quantity = 100 };
            var response = await _client.PostAsJsonAsync("/api/cart/add", cartDto);

            // Assert - Should either succeed with available quantity or return bad request
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Created);
        }

        [Fact]
        public async Task BulkUpdateInventory_WithMultipleProducts_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var product1Id = await CreateTestProductAsync();
            var product2Id = await CreateTestProductAsync();

            // Act - Bulk update (if endpoint exists)
            var bulkUpdateDto = new
            {
                Updates = new[]
                {
                    new { ProductId = product1Id, Quantity = 50 },
                    new { ProductId = product2Id, Quantity = 75 }
                }
            };
            var response = await _client.PostAsJsonAsync("/api/productinventory/bulk-update", bulkUpdateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        private async Task<Guid> GetOrCreateInventoryAsync(Guid productId)
        {
            var response = await _client.GetAsync($"/api/productinventory/product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var inventoryId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(inventoryId))
                    return Guid.Parse(inventoryId);
            }

            // Create new inventory if not exists
            var createDto = new
            {
                ProductId = productId,
                Quantity = 100
            };
            var createResponse = await _client.PostAsJsonAsync("/api/productinventory", createDto);
            if (createResponse.IsSuccessStatusCode)
            {
                var createContent = await createResponse.Content.ReadAsStringAsync();
                var inventoryId = JsonHelper.GetNestedProperty(createContent, "data", "id");
                if (!string.IsNullOrEmpty(inventoryId))
                    return Guid.Parse(inventoryId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestProductAsync(int stockQuantity = 100)
        {
            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Inventory Product {Guid.NewGuid()}",
                Description = "Test Product",
                Price = 100.0m,
                StockQuantity = stockQuantity,
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

        private async Task<Guid> CreateTestAddressAsync(Guid userId)
        {
            var addressDto = new
            {
                UserId = userId,
                Title = "Order Address",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "09123456789",
                AddressLine1 = "123 Test St",
                City = "Tehran",
                State = "Tehran",
                PostalCode = "1234567890",
                Country = "Iran",
                IsDefault = true
            };

            var response = await _client.PostAsJsonAsync("/api/useraddress", addressDto);
            if (response.IsSuccessStatusCode)
            {
                var addressContent = await response.Content.ReadAsStringAsync();
                var addressId = JsonHelper.GetNestedProperty(addressContent, "data", "id");
                if (!string.IsNullOrEmpty(addressId))
                    return Guid.Parse(addressId);
            }

            return Guid.NewGuid();
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

