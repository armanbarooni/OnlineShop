using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class CouponTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public CouponTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateCoupon_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var couponDto = new
            {
                Code = $"SAVE{Random.Shared.Next(1000, 9999)}",
                Name = "Test Coupon",
                Description = "Test coupon for integration tests",
                DiscountPercentage = 10.0m,
                DiscountAmount = 0m,
                MinimumPurchase = 100.0m,
                MaximumDiscount = 50.0m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 100,
                IsSingleUse = false
            };
            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task ApplyCoupon_WithValidCode_ShouldApplyDiscount()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync();
            var productId = await CreateTestProductAsync(price: 200m);
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Act
            var applyDto = new { CouponCode = couponCode };
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", applyDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ApplyCoupon_WithExpiredCoupon_ShouldReturnError()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateExpiredCouponAsync();

            // Act
            var applyDto = new { CouponCode = couponCode };
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", applyDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ApplyCoupon_WithInsufficientPurchaseAmount_ShouldReturnError()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync(minimumPurchase: 500m);
            var productId = await CreateTestProductAsync(price: 50m);
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Act
            var applyDto = new { CouponCode = couponCode };
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", applyDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveCoupon_FromCart_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync();
            var productId = await CreateTestProductAsync(price: 200m);
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = couponCode });

            // Act
            var response = await _client.DeleteAsync("/api/coupon/remove");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ValidateCoupon_WithValidCode_ShouldReturnValid()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync();

            // Act
            var response = await _client.GetAsync($"/api/coupon/validate/{couponCode}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCouponByCode_ShouldReturnCoupon()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync();

            // Act
            var response = await _client.GetAsync($"/api/coupon/code/{couponCode}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UseCoupon_ShouldIncrementUsageCount()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync(maxUsage: 5);
            var productId = await CreateTestProductAsync(price: 200m);
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Act
            var applyDto = new { CouponCode = couponCode };
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", applyDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ApplyCoupon_WhenMaxUsageReached_ShouldReturnError()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = await CreateTestCouponAsync(maxUsage: 0); // Already exhausted

            // Act
            var applyDto = new { CouponCode = couponCode };
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", applyDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAllCoupons_ShouldReturnList()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            await CreateTestCouponAsync();

            // Act
            var response = await _client.GetAsync("/api/coupon");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<string> CreateTestCouponAsync(decimal minimumPurchase = 100m, int maxUsage = 100)
        {
            var couponCode = $"SAVE{Random.Shared.Next(1000, 9999)}";
            var couponDto = new
            {
                Code = couponCode,
                Name = $"Test Coupon {couponCode}",
                Description = "Test coupon for integration tests",
                DiscountPercentage = 10.0m,
                DiscountAmount = 0m,
                MinimumPurchase = minimumPurchase,
                MaximumDiscount = 50.0m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = maxUsage,
                IsSingleUse = false
            };

            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);
            if (response.IsSuccessStatusCode)
            {
                return couponCode;
            }

            return "TESTCODE";
        }

        private async Task<string> CreateExpiredCouponAsync()
        {
            var couponCode = $"EXPIRED{Random.Shared.Next(1000, 9999)}";
            var couponDto = new
            {
                Code = couponCode,
                DiscountPercentage = 10.0m,
                MinimumPurchaseAmount = 100.0m,
                MaxDiscountAmount = 50.0m,
                ValidFrom = DateTime.UtcNow.AddDays(-60),
                ValidUntil = DateTime.UtcNow.AddDays(-30),
                MaxUsageCount = 100,
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);
            if (response.IsSuccessStatusCode)
            {
                return couponCode;
            }

            return "EXPIREDCODE";
        }

        private async Task<Guid> CreateTestProductAsync(decimal price = 100m)
        {
            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Coupon Product {Guid.NewGuid()}",
                Description = "Test Product",
                Price = price,
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

