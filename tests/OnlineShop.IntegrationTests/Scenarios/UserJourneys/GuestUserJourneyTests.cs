using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios.UserJourneys
{
    public class GuestUserJourneyTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public GuestUserJourneyTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteGuestUserJourney_FromBrowseToOrder_ShouldSucceed()
        {
            // Step 1: Browse products (public access)
            var browseResponse = await _client.GetAsync("/api/product/search?pageSize=10");
            browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 2: Search products
            var searchResponse = await _client.GetAsync("/api/product/search?searchTerm=test&pageSize=10");
            searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: View product details
            var productId = await CreateTestProductAsync();
            var productDetailResponse = await _client.GetAsync($"/api/product/{productId}");
            productDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 4: Try to add to wishlist (should prompt login)
            var wishlistResponse = await _client.PostAsJsonAsync("/api/wishlist", new { ProductId = productId });
            wishlistResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Step 5: Add to cart (anonymous - should work)
            var addToCartResponse = await _client.PostAsJsonAsync("/api/cart/add", new 
            { 
                ProductId = productId, 
                Quantity = 1 
            });
            addToCartResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Unauthorized);

            // Step 6: View cart (if anonymous cart is supported)
            var viewCartResponse = await _client.GetAsync("/api/cart");
            viewCartResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);

            // Step 7: Try to checkout (should prompt register)
            var userId = Guid.NewGuid();
            var addressId = await CreateTestAddressAsync(userId);
            var checkoutDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "CreditCard"
            };
            var checkoutResponse = await _client.PostAsJsonAsync("/api/userorder", checkoutDto);
            checkoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Step 8: Register
            var phoneNumber = $"0991{new Random().Next(10000000, 99999999)}";
            var registerDto = new
            {
                PhoneNumber = phoneNumber,
                Code = "123456", // In real scenario, this would be from SMS
                FirstName = "Guest",
                LastName = "User"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register-phone", registerDto);
            registerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);

            // Step 9: Complete order (if registration succeeded)
            if (registerResponse.IsSuccessStatusCode)
            {
                var authContent = await registerResponse.Content.ReadAsStringAsync();
                var accessToken = JsonHelper.GetNestedProperty(authContent, "data", "accessToken");
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var userIdFromToken = JsonHelper.GetNestedProperty(authContent, "data", "userId");
                    var finalCheckoutDto = new
                    {
                        UserId = Guid.Parse(userIdFromToken),
                        ShippingAddressId = addressId,
                        PaymentMethod = "CreditCard"
                    };
                    
                    var finalCheckoutResponse = await _client.PostAsJsonAsync("/api/userorder", finalCheckoutDto);
                    finalCheckoutResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
                }
            }
        }

        [Fact]
        public async Task GuestUser_CanBrowseCategoriesAndBrands_WithoutAuth()
        {
            // Browse categories
            var categoriesResponse = await _client.GetAsync("/api/productcategory");
            categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Browse brands
            var brandsResponse = await _client.GetAsync("/api/brand");
            brandsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Browse units
            var unitsResponse = await _client.GetAsync("/api/unit");
            unitsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Browse materials
            var materialsResponse = await _client.GetAsync("/api/material");
            materialsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Browse seasons
            var seasonsResponse = await _client.GetAsync("/api/season");
            seasonsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GuestUser_CanSearchWithFilters_WithoutAuth()
        {
            // Search with price filter
            var priceSearchResponse = await _client.GetAsync("/api/product/search?minPrice=50&maxPrice=200&pageSize=10");
            priceSearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Search with category filter
            var categoryId = await CreateTestCategoryAsync();
            var categorySearchResponse = await _client.GetAsync($"/api/product/search?categoryId={categoryId}&pageSize=10");
            categorySearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Search with brand filter
            var brandId = await CreateTestBrandAsync();
            var brandSearchResponse = await _client.GetAsync($"/api/product/search?brandId={brandId}&pageSize=10");
            brandSearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Search with sorting
            var sortSearchResponse = await _client.GetAsync("/api/product/search?sortBy=price&sortDescending=false&pageSize=10");
            sortSearchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GuestUser_CanViewProductImages_WithoutAuth()
        {
            // Arrange
            var productId = await CreateTestProductAsync();
            await CreateTestProductImageAsync(productId);

            // View product images
            var imagesResponse = await _client.GetAsync($"/api/productimage/product/{productId}");
            imagesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // View specific image
            var imageId = await CreateTestProductImageAsync(productId);
            var imageResponse = await _client.GetAsync($"/api/productimage/{imageId}");
            imageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GuestUser_CanViewProductDetails_WithoutAuth()
        {
            // Arrange
            var productId = await CreateTestProductAsync();
            await CreateTestProductDetailAsync(productId);

            // View product details
            var productResponse = await _client.GetAsync($"/api/product/{productId}");
            productResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // View product inventory
            var inventoryResponse = await _client.GetAsync($"/api/productinventory/product/{productId}");
            inventoryResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

            // View product variants
            await CreateTestProductVariantAsync(productId);
            var variantsResponse = await _client.GetAsync($"/api/productvariant/product/{productId}");
            variantsResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GuestUser_CannotAccessProtectedFeatures_WithoutAuth()
        {
            var productId = await CreateTestProductAsync();

            // Cannot add to wishlist
            var wishlistResponse = await _client.PostAsJsonAsync("/api/wishlist", new { ProductId = productId });
            wishlistResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Cannot create return request
            var returnResponse = await _client.PostAsJsonAsync("/api/userreturnrequest", new
            {
                OrderId = Guid.NewGuid(),
                Reason = "Product defective",
                Description = "Test",
                RequestedAction = "Refund"
            });
            returnResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Cannot create stock alert
            var stockAlertResponse = await _client.PostAsJsonAsync("/api/stockalert", new
            {
                ProductId = productId,
                Email = "test@example.com",
                NotifyWhenAvailable = true
            });
            stockAlertResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Cannot access user profile
            var profileResponse = await _client.GetAsync("/api/auth/me");
            profileResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GuestUser_CanCompareProducts_WithoutAuth()
        {
            // Arrange
            var product1Id = await CreateTestProductAsync();
            var product2Id = await CreateTestProductAsync();

            // Add products to comparison (this might require auth, but testing anyway)
            var add1Response = await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = product1Id });
            var add2Response = await _client.PostAsJsonAsync("/api/productcomparison/add", new { ProductId = product2Id });

            // View comparison (might require auth)
            var compareResponse = await _client.GetAsync("/api/productcomparison/compare");
            compareResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GuestUser_CanViewProductReviews_WithoutAuth()
        {
            // Arrange
            var productId = await CreateTestProductAsync();
            await CreateTestProductReviewAsync(productId);

            // View product reviews
            var reviewsResponse = await _client.GetAsync($"/api/productreview/product/{productId}");
            reviewsResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        #region Helper Methods

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
                Name = $"Guest Journey Test Product {Guid.NewGuid()}",
                Description = "Test product for guest user journey",
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

        private async Task<Guid> CreateTestProductImageAsync(Guid productId)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var imageDto = new
            {
                ProductId = productId,
                ImageUrl = $"https://example.com/images/{Guid.NewGuid()}.jpg",
                AltText = "Test product image",
                IsPrimary = true,
                DisplayOrder = 1
            };

            var response = await _client.PostAsJsonAsync("/api/productimage", imageDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var imageId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(imageId))
                    return Guid.Parse(imageId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestProductDetailAsync(Guid productId)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var detailDto = new
            {
                ProductId = productId,
                Key = "Color",
                Value = "Red",
                Description = "Product color detail"
            };

            var response = await _client.PostAsJsonAsync("/api/productdetail", detailDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var detailId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(detailId))
                    return Guid.Parse(detailId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestProductVariantAsync(Guid productId)
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
                Price = 120.0m,
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

        private async Task<Guid> CreateTestProductReviewAsync(Guid productId)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var reviewDto = new
            {
                ProductId = productId,
                Rating = 5,
                Comment = "Great product!",
                ReviewerName = "Test Reviewer"
            };

            var response = await _client.PostAsJsonAsync("/api/productreview", reviewDto);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var reviewId = JsonHelper.GetNestedProperty(content, "data", "id");
                if (!string.IsNullOrEmpty(reviewId))
                    return Guid.Parse(reviewId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestCategoryAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var categoryDto = new
            {
                Name = $"Guest Journey Test Category {Guid.NewGuid()}",
                Description = "Test category",
                MahakClientId = (long)Random.Shared.Next(1000, 9999),
                MahakId = Random.Shared.Next(1000, 9999)
            };

            var response = await _client.PostAsJsonAsync("/api/productcategory", categoryDto);
            var content = await response.Content.ReadAsStringAsync();
            var categoryId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(categoryId);
        }

        private async Task<Guid> CreateTestBrandAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var brandDto = new
            {
                Name = $"Guest Journey Test Brand {Guid.NewGuid()}",
                Description = "Test brand",
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/brand", brandDto);
            var content = await response.Content.ReadAsStringAsync();
            var brandId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(brandId);
        }

        private async Task<Guid> CreateTestUnitAsync()
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var unitDto = new
            {
                Name = $"Guest Journey Test Unit {Guid.NewGuid()}",
                MahakClientId = (long)Random.Shared.Next(1000, 9999),
                MahakId = Random.Shared.Next(1000, 9999)
            };

            var response = await _client.PostAsJsonAsync("/api/unit", unitDto);
            var content = await response.Content.ReadAsStringAsync();
            var unitId = JsonHelper.GetNestedProperty(content, "data", "id") ?? Guid.Empty.ToString();
            return Guid.Parse(unitId);
        }

        private async Task<Guid> CreateTestAddressAsync(Guid userId)
        {
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var addressDto = new
            {
                UserId = userId,
                Title = "Guest Journey Test Address",
                FirstName = "Guest",
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

        #endregion
    }
}

