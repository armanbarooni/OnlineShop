using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Security
{
    public class AuthorizationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthorizationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        #region Admin Only Endpoints Tests

        [Fact]
        public async Task CreateProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = "Test Product",
                Description = "Test description",
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
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            var updateDto = new
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated description",
                Price = 150.0m,
                StockQuantity = 50,
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/product/{productId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DeleteProduct_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ManageInventory_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            var inventoryDto = new
            {
                ProductId = productId,
                AvailableQuantity = 50,
                ReservedQuantity = 10,
                MinimumStockLevel = 5
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/productinventory", inventoryDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UpdateOrderStatus_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();

            var updateDto = new
            {
                OrderStatus = "Processing"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/userorder/{orderId}", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ManageCoupons_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponDto = new
            {
                Code = $"USERCOUPON{Random.Shared.Next(1000, 9999)}",
                Name = "User Coupon",
                Description = "Test coupon",
                DiscountPercentage = 10.0m,
                MinimumPurchase = 100.0m,
                MaximumDiscount = 50.0m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 100,
                IsSingleUse = false
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AccessMahakIntegration_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/mahakmapping");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region User Endpoints Tests

        [Fact]
        public async Task AccessOwnProfile_AsUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AccessOwnOrders_AsUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/userorder/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AccessOwnCart_AsUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AccessOwnAddresses_AsUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/useraddress/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AccessOtherUserOrders_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var otherUserId = Guid.NewGuid(); // Different user ID

            // Act
            var response = await _client.GetAsync($"/api/userorder/user/{otherUserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AccessOtherUserAddresses_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var otherUserId = Guid.NewGuid(); // Different user ID

            // Act
            var response = await _client.GetAsync($"/api/useraddress/user/{otherUserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        #endregion

        #region Public Endpoints Tests

        [Fact]
        public async Task SearchProducts_WithoutAuth_ShouldSucceed()
        {
            // Act
            var response = await _client.GetAsync("/api/product/search?searchTerm=test&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetProductDetails_WithoutAuth_ShouldSucceed()
        {
            // Arrange
            var productId = await CreateTestProductAsync();

            // Act
            var response = await _client.GetAsync($"/api/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCategories_WithoutAuth_ShouldSucceed()
        {
            // Act
            var response = await _client.GetAsync("/api/productcategory");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBrands_WithoutAuth_ShouldSucceed()
        {
            // Act
            var response = await _client.GetAsync("/api/brand");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUnits_WithoutAuth_ShouldSucceed()
        {
            // Act
            var response = await _client.GetAsync("/api/unit");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion

        #region Anonymous Access Tests

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateOrder_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var orderDto = new
            {
                UserId = Guid.NewGuid(),
                ShippingAddressId = Guid.NewGuid(),
                PaymentMethod = "CreditCard"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddToCart_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var cartDto = new
            {
                ProductId = Guid.NewGuid(),
                Quantity = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart/add", cartDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReturnRequest_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var returnDto = new
            {
                OrderId = Guid.NewGuid(),
                Reason = "Product defective",
                Description = "The product arrived damaged",
                RequestedAction = "Refund"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userreturnrequest", returnDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Token Validation Tests

        [Fact]
        public async Task AccessProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-token");

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithMalformedToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "malformed.token");

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Role-Based Access Tests

        [Fact]
        public async Task AdminCanAccessAllEndpoints_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Test various admin endpoints
            var endpoints = new[]
            {
                "/api/product",
                "/api/productinventory",
                "/api/coupon",
                "/api/mahakmapping",
                "/api/userorder",
                "/api/userreturnrequest"
            };

            foreach (var endpoint in endpoints)
            {
                // Act
                var response = await _client.GetAsync(endpoint);

                // Assert
                response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, 
                    $"Admin should have access to {endpoint}");
            }
        }

        [Fact]
        public async Task UserCannotAccessAdminEndpoints_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Test various admin-only endpoints
            var adminEndpoints = new[]
            {
                "/api/product",
                "/api/productinventory",
                "/api/coupon",
                "/api/mahakmapping"
            };

            foreach (var endpoint in adminEndpoints)
            {
                // Act
                var response = await _client.GetAsync(endpoint);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden, 
                    $"User should not have access to {endpoint}");
            }
        }

        #endregion

        #region Helper Methods

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
                Name = $"Authorization Test Product {Guid.NewGuid()}",
                Description = "Test product for authorization tests",
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

        private async Task<Guid> CreateTestOrderAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Add to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Create order
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };

            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);
            if (response.IsSuccessStatusCode)
            {
                var orderContent = await response.Content.ReadAsStringAsync();
                var orderId = JsonHelper.GetNestedProperty(orderContent, "data", "id");
                if (!string.IsNullOrEmpty(orderId))
                    return Guid.Parse(orderId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Authorization Test Category {Guid.NewGuid()}",
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
                Name = $"Authorization Test Unit {Guid.NewGuid()}",
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
                Name = $"Authorization Test Brand {Guid.NewGuid()}",
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
                Title = "Authorization Test Address",
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

        #endregion
    }
}

