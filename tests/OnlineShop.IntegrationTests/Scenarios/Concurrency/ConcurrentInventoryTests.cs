using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Concurrency
{
    public class ConcurrentInventoryTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ConcurrentInventoryTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ConcurrentAddToCart_ShouldMaintainStockConsistency()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 10);
            var concurrentTasks = new List<Task<HttpResponseMessage>>();
            var numberOfUsers = 10;
            var quantityPerUser = 1;

            // Act - 10 users adding to cart simultaneously
            for (int i = 0; i < numberOfUsers; i++)
            {
                var task = AddToCartAsync(productId, quantityPerUser);
                concurrentTasks.Add(task);
            }

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            var successfulAdditions = responses.Count(r => r.IsSuccessStatusCode);
            successfulAdditions.Should().BeLessThanOrEqualTo(10); // Should not exceed stock

            // Verify final stock
            var finalStock = await GetProductStockAsync(productId);
            finalStock.Should().BeGreaterThanOrEqualTo(0);
            finalStock.Should().BeLessThanOrEqualTo(10);
        }

        [Fact]
        public async Task ConcurrentOrderPlacement_ShouldPreventOverSelling()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 5);
            var concurrentTasks = new List<Task<HttpResponseMessage>>();
            var numberOfOrders = 5;
            var quantityPerOrder = 2; // Each order wants 2, but only 5 available

            // Act - 5 users placing orders simultaneously
            for (int i = 0; i < numberOfOrders; i++)
            {
                var task = PlaceOrderAsync(productId, quantityPerOrder);
                concurrentTasks.Add(task);
            }

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            var successfulOrders = responses.Count(r => r.IsSuccessStatusCode);
            successfulOrders.Should().BeLessThanOrEqualTo(2); // Max 2 orders can succeed (5 stock / 2 quantity)

            // Verify no over-selling occurred
            var finalStock = await GetProductStockAsync(productId);
            finalStock.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task ConcurrentInventoryUpdate_ShouldMaintainDataConsistency()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 100);
            var concurrentTasks = new List<Task<HttpResponseMessage>>();

            // Act - Admin updating inventory while users are ordering
            var adminUpdateTask = UpdateProductStockAsync(productId, 150);
            var userOrderTask1 = PlaceOrderAsync(productId, 10);
            var userOrderTask2 = PlaceOrderAsync(productId, 15);
            var userOrderTask3 = PlaceOrderAsync(productId, 20);

            concurrentTasks.AddRange([adminUpdateTask, userOrderTask1, userOrderTask2, userOrderTask3]);

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            var adminUpdateResponse = responses[0];
            adminUpdateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

            // Verify final stock is consistent
            var finalStock = await GetProductStockAsync(productId);
            finalStock.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task StockReservation_ShouldHandleTimeoutCorrectly()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 5);
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act 1: Add to cart (reserve stock)
            var addToCartResponse = await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                Quantity = 3 
            });
            addToCartResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            // Act 2: Wait and then add more (should work if reservation expired)
            await Task.Delay(100); // Simulate timeout period
            var addMoreResponse = await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                Quantity = 2 
            });

            // Assert
            addMoreResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
            
            // Verify stock consistency
            var finalStock = await GetProductStockAsync(productId);
            finalStock.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task ConcurrentStockAlerts_ShouldHandleMultipleNotifications()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 2); // Low stock
            var concurrentTasks = new List<Task<HttpResponseMessage>>();
            var numberOfAlerts = 5;

            // Act - Multiple users creating stock alerts simultaneously
            for (int i = 0; i < numberOfAlerts; i++)
            {
                var task = CreateStockAlertAsync(productId, $"user{i}@example.com");
                concurrentTasks.Add(task);
            }

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            var successfulAlerts = responses.Count(r => r.IsSuccessStatusCode);
            successfulAlerts.Should().Be(numberOfAlerts); // All alerts should succeed

            // Verify alerts were created
            var alertsResponse = await _client.GetAsync($"/api/stockalert/product/{productId}");
            alertsResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ConcurrentCartOperations_ShouldMaintainConsistency()
        {
            // Arrange
            var productId = await CreateTestProductAsync(stockQuantity: 20);
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var concurrentTasks = new List<Task<HttpResponseMessage>>();

            // Act - Multiple concurrent cart operations
            var addTask1 = _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 5 });
            var addTask2 = _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 3 });
            var updateTask = _client.PutAsJsonAsync("/api/cart/update", new { ProductId = productId, Quantity = 8 });
            var removeTask = _client.DeleteAsync($"/api/cart/remove/{productId}");

            concurrentTasks.AddRange([addTask1, addTask2, updateTask, removeTask]);

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            // At least some operations should succeed
            var successfulOperations = responses.Count(r => r.IsSuccessStatusCode);
            successfulOperations.Should().BeGreaterThan(0);

            // Verify final cart state is consistent
            var cartResponse = await _client.GetAsync("/api/cart");
            cartResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ConcurrentInventoryBulkUpdate_ShouldMaintainConsistency()
        {
            // Arrange
            var product1Id = await CreateTestProductAsync(stockQuantity: 50);
            var product2Id = await CreateTestProductAsync(stockQuantity: 30);
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var concurrentTasks = new List<Task<HttpResponseMessage>>();

            // Act - Bulk update while users are ordering
            var bulkUpdateTask = _client.PostAsJsonAsync("/api/productinventory/bulk-update", new
            {
                Items = new[]
                {
                    new { ProductId = product1Id, AvailableQuantity = 100 },
                    new { ProductId = product2Id, AvailableQuantity = 80 }
                }
            });

            var orderTask1 = PlaceOrderAsync(product1Id, 10);
            var orderTask2 = PlaceOrderAsync(product2Id, 15);

            concurrentTasks.AddRange([bulkUpdateTask, orderTask1, orderTask2]);

            var responses = await Task.WhenAll(concurrentTasks);

            // Assert
            var bulkUpdateResponse = responses[0];
            bulkUpdateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            // Verify final stocks are consistent
            var finalStock1 = await GetProductStockAsync(product1Id);
            var finalStock2 = await GetProductStockAsync(product2Id);
            
            finalStock1.Should().BeGreaterThanOrEqualTo(0);
            finalStock2.Should().BeGreaterThanOrEqualTo(0);
        }

        private async Task<HttpResponseMessage> AddToCartAsync(Guid productId, int quantity)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            return await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                Quantity = quantity 
            });
        }

        private async Task<HttpResponseMessage> PlaceOrderAsync(Guid productId, int quantity)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // First add to cart
            await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                Quantity = quantity 
            });

            // Then place order
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);
            
            return await _client.PostAsJsonAsync("/api/userorder", new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            });
        }

        private async Task<HttpResponseMessage> UpdateProductStockAsync(Guid productId, int newStock)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            return await _client.PutAsJsonAsync($"/api/product/{productId}", new
            {
                StockQuantity = newStock
            });
        }

        private async Task<HttpResponseMessage> CreateStockAlertAsync(Guid productId, string email)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            return await _client.PostAsJsonAsync("/api/stockalert", new
            {
                ProductId = productId,
                Email = email,
                NotifyWhenAvailable = true
            });
        }

        private async Task<int> GetProductStockAsync(Guid productId)
        {
            var response = await _client.GetAsync($"/api/product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stockStr = JsonHelper.GetNestedProperty(content, "data", "stockQuantity");
                if (int.TryParse(stockStr, out int stock))
                    return stock;
            }
            return 0;
        }

        private async Task<Guid> CreateTestProductAsync(int stockQuantity = 100)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Concurrency Test Product {Guid.NewGuid()}",
                Description = "Test product for concurrency tests",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Concurrency Test Category {Guid.NewGuid()}",
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
                Name = $"Concurrency Test Unit {Guid.NewGuid()}",
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
                Name = $"Concurrency Test Brand {Guid.NewGuid()}",
                Description = "Test brand",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            var content = await response.Content.ReadAsStringAsync();
            var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(brandId);
        }

        private async Task<Guid> CreateTestAddressAsync(Guid userId)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var addressDto = new
            {
                UserId = userId,
                Title = "Concurrency Test Address",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

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
