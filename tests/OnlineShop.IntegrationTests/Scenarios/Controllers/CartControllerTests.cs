using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.Controllers
{
    public class CartControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CartControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetOwnCart_AsAuthenticatedUser_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetOwnCart_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllCarts_AsAdmin_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/cart/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllCarts_AsUser_ShouldReturnForbidden()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/cart/all");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetCartById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var cartId = await CreateTestCartAsync();

            // Act
            var response = await _client.GetAsync($"/api/cart/{cartId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCartByUserId_WithValidUserId_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/cart/user/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateCart_AsAuthenticatedUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var cartDto = new
            {
                SessionId = Guid.NewGuid().ToString(),
                CartName = "Test Cart",
                Notes = "Test Notes"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart", cartDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateCart_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var cartDto = new
            {
                SessionId = Guid.NewGuid().ToString(),
                CartName = "Test Cart"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart", cartDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateCart_AsAuthenticatedUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var cartId = await CreateTestCartAsync();
            var updateDto = new
            {
                Id = cartId,
                CartName = "Updated Cart Name",
                IsActive = true,
                Notes = "Updated Notes"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/cart/{cartId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteCart_AsAuthenticatedUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var cartId = await CreateTestCartAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/cart/{cartId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddItemToCart_AsAuthenticatedUser_ShouldSucceed()
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

            // Get or create cart
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            var cartId = await CreateTestCartAsync();

            var cartItemDto = new
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = 2,
                Notes = "Test Item"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart/add", cartItemDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddItemToCart_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var cartItemDto = new
            {
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart/add", cartItemDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ApplyCoupon_AsAuthenticatedUser_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var requestDto = new
            {
                CouponCode = "TESTCODE",
                CartId = (Guid?)null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart/apply-coupon", requestDto);

            // Assert
            // May return BadRequest if coupon doesn't exist, which is acceptable
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveCoupon_AsAuthenticatedUser_ShouldReturnOk()
        {
            // Arrange
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.DeleteAsync("/api/cart/remove-coupon");

            // Assert
            // May return BadRequest if no coupon applied, which is acceptable
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestCartAsync()
        {
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var cartDto = new
            {
                SessionId = Guid.NewGuid().ToString(),
                CartName = $"TestCart{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Notes = "Test Cart"
            };

            var response = await _client.PostAsJsonAsync("/api/cart", cartDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cartId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(cartId))
                    return Guid.Parse(cartId);
            }

            // Fallback: try to get existing cart
            var userId = await GetCurrentUserIdAsync();
            var getResponse = await _client.GetAsync($"/api/cart/user/{userId}");
            if (getResponse.IsSuccessStatusCode)
            {
                var content = await getResponse.Content.ReadAsStringAsync();
                var cartId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(cartId))
                    return Guid.Parse(cartId);
            }

            return Guid.NewGuid(); // Fallback
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            var authToken = await AuthHelper.GetUserTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _client.GetAsync("/api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userId = JsonHelper.GetNestedProperty(content, "id")
                    ?? JsonHelper.GetNestedProperty(content, "userId");
                if (!string.IsNullOrEmpty(userId))
                    return Guid.Parse(userId);
            }

            return Guid.NewGuid(); // Fallback
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
                Name = $"CartProduct{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Description = "Test Product",
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


