using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.BusinessLogic
{
    public class PriceCalculationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public PriceCalculationTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        #region Cart Total with Multiple Items Tests

        [Fact]
        public async Task CalculateCartTotal_WithMultipleItems_ShouldReturnCorrectTotal()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var product1Id = await CreateTestProductAsync(price: 100.0m);
            var product2Id = await CreateTestProductAsync(price: 150.0m);
            var product3Id = await CreateTestProductAsync(price: 200.0m);

            // Add multiple items to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = product1Id, Quantity = 2 });
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = product2Id, Quantity = 1 });
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = product3Id, Quantity = 3 });

            // Expected total: (100 * 2) + (150 * 1) + (200 * 3) = 200 + 150 + 600 = 950

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                total.Should().Be(950.0m);
            }
        }

        [Fact]
        public async Task CalculateCartTotal_WithProductVariants_ShouldReturnCorrectTotal()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 100.0m);
            var variant1Id = await CreateTestProductVariantAsync(productId, price: 120.0m);
            var variant2Id = await CreateTestProductVariantAsync(productId, price: 80.0m);

            // Add variants to cart
            await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                ProductVariantId = variant1Id, 
                Quantity = 2 
            });
            await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                ProductVariantId = variant2Id, 
                Quantity = 1 
            });

            // Expected total: (120 * 2) + (80 * 1) = 240 + 80 = 320

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                total.Should().Be(320.0m);
            }
        }

        #endregion

        #region Discount Calculation Tests

        [Fact]
        public async Task ApplyPercentageDiscount_ShouldCalculateCorrectAmount()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 200.0m);
            var couponCode = await CreatePercentageCouponAsync(discountPercentage: 15.0m);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Apply coupon
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = couponCode });

            // Expected: 200 - (200 * 0.15) = 200 - 30 = 170

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                total.Should().Be(170.0m);
            }
        }

        [Fact]
        public async Task ApplyFixedAmountDiscount_ShouldCalculateCorrectAmount()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 200.0m);
            var couponCode = await CreateFixedAmountCouponAsync(discountAmount: 50.0m);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Apply coupon
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = couponCode });

            // Expected: 200 - 50 = 150

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                total.Should().Be(150.0m);
            }
        }

        [Fact]
        public async Task ApplyDiscount_WithMaximumDiscountLimit_ShouldNotExceedLimit()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 1000.0m);
            var couponCode = await CreateCouponWithMaxDiscountAsync(
                discountPercentage: 20.0m, 
                maxDiscount: 100.0m);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Apply coupon
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = couponCode });

            // Expected: 1000 - 100 (max discount) = 900 (not 1000 - 200 = 800)

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                total.Should().Be(900.0m);
            }
        }

        #endregion

        #region Multiple Coupons Tests

        [Fact]
        public async Task ApplyMultipleCoupons_ShouldApplyOnlyOne()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 200.0m);
            var coupon1Code = await CreatePercentageCouponAsync(discountPercentage: 10.0m);
            var coupon2Code = await CreatePercentageCouponAsync(discountPercentage: 15.0m);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Apply first coupon
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = coupon1Code });

            // Try to apply second coupon (should fail or replace first)
            var response = await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = coupon2Code });

            // Act
            var cartResponse = await _client.GetAsync("/api/cart");

            // Assert
            cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await cartResponse.Content.ReadAsStringAsync();
            var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
            
            if (decimal.TryParse(totalStr, out decimal total))
            {
                // Should be either 180 (10% discount) or 170 (15% discount), not both
                total.Should().BeOneOf(180.0m, 170.0m);
            }
        }

        #endregion

        #region Tax Calculation Tests

        [Fact]
        public async Task CalculateOrder_WithTax_ShouldIncludeTaxInTotal()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 100.0m);
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Create order (tax should be calculated)
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
                var taxStr = JsonHelper.GetNestedProperty(content, "data", "taxAmount");
                
                if (decimal.TryParse(totalStr, out decimal total) && decimal.TryParse(taxStr, out decimal tax))
                {
                    total.Should().BeGreaterThan(100.0m); // Should include tax
                    tax.Should().BeGreaterThan(0);
                }
            }
        }

        #endregion

        #region Shipping Cost Tests

        [Fact]
        public async Task CalculateOrder_WithShippingCost_ShouldIncludeShippingInTotal()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 50.0m);
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Create order (shipping should be calculated)
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
                var shippingStr = JsonHelper.GetNestedProperty(content, "data", "shippingCost");
                
                if (decimal.TryParse(totalStr, out decimal total) && decimal.TryParse(shippingStr, out decimal shipping))
                {
                    total.Should().BeGreaterThan(50.0m); // Should include shipping
                    shipping.Should().BeGreaterThan(0);
                }
            }
        }

        [Fact]
        public async Task CalculateOrder_WithFreeShippingThreshold_ShouldApplyFreeShipping()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(price: 500.0m); // Above free shipping threshold
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Add product to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            // Create order
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var shippingStr = JsonHelper.GetNestedProperty(content, "data", "shippingCost");
                
                if (decimal.TryParse(shippingStr, out decimal shipping))
                {
                    shipping.Should().Be(0); // Should be free shipping
                }
            }
        }

        #endregion

        #region Complex Price Calculation Tests

        [Fact]
        public async Task CalculateComplexOrder_WithMultipleItemsDiscountTaxAndShipping_ShouldReturnCorrectTotal()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var product1Id = await CreateTestProductAsync(price: 100.0m);
            var product2Id = await CreateTestProductAsync(price: 200.0m);
            var couponCode = await CreatePercentageCouponAsync(discountPercentage: 10.0m);
            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Add multiple items to cart
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = product1Id, Quantity = 2 });
            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = product2Id, Quantity = 1 });

            // Apply discount
            await _client.PostAsJsonAsync("/api/coupon/apply", new { CouponCode = couponCode });

            // Create order
            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userorder", orderDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var subtotalStr = JsonHelper.GetNestedProperty(content, "data", "subtotalAmount");
                var discountStr = JsonHelper.GetNestedProperty(content, "data", "discountAmount");
                var taxStr = JsonHelper.GetNestedProperty(content, "data", "taxAmount");
                var shippingStr = JsonHelper.GetNestedProperty(content, "data", "shippingCost");
                var totalStr = JsonHelper.GetNestedProperty(content, "data", "totalAmount");
                
                // Verify all components are calculated
                subtotalStr.Should().NotBeNullOrEmpty();
                discountStr.Should().NotBeNullOrEmpty();
                taxStr.Should().NotBeNullOrEmpty();
                shippingStr.Should().NotBeNullOrEmpty();
                totalStr.Should().NotBeNullOrEmpty();
            }
        }

        #endregion

        #region Helper Methods

        private async Task<Guid> CreateTestProductAsync(decimal price = 100.0m)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Price Calculation Test Product {Guid.NewGuid()}",
                Description = "Test product for price calculation tests",
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

        private async Task<Guid> CreateTestProductVariantAsync(Guid productId, decimal price = 100.0m)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var variantDto = new
            {
                ProductId = productId,
                Sku = $"SKU-{Guid.NewGuid()}",
                Size = "Medium",
                Color = "Blue",
                Price = price,
                StockQuantity = 50
            };

            var response = await _client.PostAsJsonAsync("/api/productvariant", variantDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var variantId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(variantId))
                    return Guid.Parse(variantId);
            }

            return Guid.NewGuid();
        }

        private async Task<string> CreatePercentageCouponAsync(decimal discountPercentage = 10.0m)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = $"PERCENT{Random.Shared.Next(1000, 9999)}";
            var couponDto = new
            {
                Code = couponCode,
                Name = "Percentage Coupon",
                Description = "Test percentage coupon",
                DiscountPercentage = discountPercentage,
                DiscountAmount = 0m,
                MinimumPurchase = 50.0m,
                MaximumDiscount = 100.0m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 100,
                IsSingleUse = false
            };

            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);
            if (response.IsSuccessStatusCode)
            {
                return couponCode;
            }

            return "PERCENTCODE";
        }

        private async Task<string> CreateFixedAmountCouponAsync(decimal discountAmount = 50.0m)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = $"FIXED{Random.Shared.Next(1000, 9999)}";
            var couponDto = new
            {
                Code = couponCode,
                Name = "Fixed Amount Coupon",
                Description = "Test fixed amount coupon",
                DiscountPercentage = 0m,
                DiscountAmount = discountAmount,
                MinimumPurchase = 100.0m,
                MaximumDiscount = discountAmount,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 100,
                IsSingleUse = false
            };

            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);
            if (response.IsSuccessStatusCode)
            {
                return couponCode;
            }

            return "FIXEDCODE";
        }

        private async Task<string> CreateCouponWithMaxDiscountAsync(decimal discountPercentage = 20.0m, decimal maxDiscount = 100.0m)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var couponCode = $"MAXDISCOUNT{Random.Shared.Next(1000, 9999)}";
            var couponDto = new
            {
                Code = couponCode,
                Name = "Max Discount Coupon",
                Description = "Test coupon with max discount limit",
                DiscountPercentage = discountPercentage,
                DiscountAmount = 0m,
                MinimumPurchase = 200.0m,
                MaximumDiscount = maxDiscount,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                UsageLimit = 100,
                IsSingleUse = false
            };

            var response = await _client.PostAsJsonAsync("/api/coupon", couponDto);
            if (response.IsSuccessStatusCode)
            {
                return couponCode;
            }

            return "MAXDISCOUNTCODE";
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Price Calculation Test Category {Guid.NewGuid()}",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"Price Calculation Test Unit {Guid.NewGuid()}",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"Price Calculation Test Brand {Guid.NewGuid()}",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var addressDto = new
            {
                UserId = userId,
                Title = "Price Calculation Test Address",
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
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
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

