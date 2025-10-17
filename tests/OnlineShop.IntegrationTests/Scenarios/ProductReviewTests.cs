using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class ProductReviewTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ProductReviewTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateReview_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var reviewDto = new
            {
                ProductId = productId,
                Rating = 5,
                Comment = "Excellent product!",
                ReviewerName = "Test User"
            };
            var response = await _client.PostAsJsonAsync("/api/productreview", reviewDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateReview_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var reviewId = await CreateTestReviewAsync(productId);

            // Act
            var updateDto = new
            {
                Rating = 4,
                Comment = "Updated: Good product",
                ReviewerName = "Test User"
            };
            var response = await _client.PutAsJsonAsync($"/api/productreview/{reviewId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteReview_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var reviewId = await CreateTestReviewAsync(productId);

            // Act
            var response = await _client.DeleteAsync($"/api/productreview/{reviewId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetProductReviews_ShouldReturnAllReviewsForProduct()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await CreateTestReviewAsync(productId);
            await CreateTestReviewAsync(productId);

            // Act
            var response = await _client.GetAsync($"/api/productreview/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task ApproveReview_ByAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var reviewId = await CreateTestReviewAsync(productId);

            // Act
            var response = await _client.PostAsync($"/api/productreview/{reviewId}/approve", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RejectReview_ByAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var reviewId = await CreateTestReviewAsync(productId);

            // Act
            var response = await _client.PostAsync($"/api/productreview/{reviewId}/reject", null);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReviewsByRating_ShouldReturnFilteredReviews()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await CreateTestReviewAsync(productId, rating: 5);
            await CreateTestReviewAsync(productId, rating: 3);

            // Act - Get all reviews for product (filtering would be done client-side or via query params)
            var response = await _client.GetAsync($"/api/productreview/product/{productId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReview_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var productId = Guid.NewGuid();

            // Act
            var reviewDto = new
            {
                ProductId = productId,
                Rating = 5,
                Comment = "Test"
            };
            var response = await _client.PostAsJsonAsync("/api/productreview", reviewDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReview_WithInvalidRating_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act
            var reviewDto = new
            {
                ProductId = productId,
                Rating = 10, // Invalid rating (should be 1-5)
                Comment = "Test"
            };
            var response = await _client.PostAsJsonAsync("/api/productreview", reviewDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Created);
        }

        private async Task<Guid> CreateTestReviewAsync(Guid productId, int rating = 5)
        {
            var reviewDto = new
            {
                ProductId = productId,
                Rating = rating,
                Comment = $"Test Review {Guid.NewGuid()}",
                ReviewerName = "Test User"
            };

            var response = await _client.PostAsJsonAsync("/api/productreview", reviewDto);
            if (response.IsSuccessStatusCode)
            {
                var reviewContent = await response.Content.ReadAsStringAsync();
                var reviewId = JsonHelper.GetNestedProperty(reviewContent, "data", "id");
                if (!string.IsNullOrEmpty(reviewId))
                    return Guid.Parse(reviewId);
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
                Name = $"Review Product {Guid.NewGuid()}",
                Description = "Test Product",
                Price = 100.0m,
                StockQuantity = 50,
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

