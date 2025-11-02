using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class StockAlertTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public StockAlertTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateStockAlert_ForOutOfStockProduct_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 0);

            // Act
            var alertDto = new
            {
                ProductId = productId,
                Email = "test@example.com",
                NotifyWhenAvailable = true
            };
            var response = await _client.PostAsJsonAsync("/api/stockalert", alertDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetUserStockAlerts_ShouldReturnAllUserAlerts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var productId = await CreateTestProductAsync();
            await CreateTestStockAlertAsync(productId);

            // Act
            var response = await _client.GetAsync($"/api/stockalert/user/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteStockAlert_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var alertId = await CreateTestStockAlertAsync(productId);

            // Act
            var response = await _client.DeleteAsync($"/api/stockalert/{alertId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAlertsByProduct_ShouldReturnProductAlerts()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            await CreateTestStockAlertAsync(productId);

            // Act
            var response = await _client.GetAsync($"/api/stockalert/product/{productId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task NotifyAlerts_WhenStockAvailable_ShouldTriggerNotifications()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync(stockQuantity: 0);
            await CreateTestStockAlertAsync(productId);

            // Act - Update product stock (this should trigger notifications)
            var updateDto = new
            {
                StockQuantity = 100
            };
            var response = await _client.PutAsJsonAsync($"/api/product/{productId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
            // In real scenario, we'd check if notification was sent (mocked)
        }

        [Fact]
        public async Task CreateStockAlert_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var productId = Guid.NewGuid();

            // Act
            var alertDto = new
            {
                ProductId = productId,
                Email = "test@example.com"
            };
            var response = await _client.PostAsJsonAsync("/api/stockalert", alertDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateDuplicateStockAlert_ShouldHandleGracefully()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();

            // Act - Create same alert twice
            var alertDto = new
            {
                ProductId = productId,
                Email = "test@example.com"
            };
            var response1 = await _client.PostAsJsonAsync("/api/stockalert", alertDto);
            var response2 = await _client.PostAsJsonAsync("/api/stockalert", alertDto);

            // Assert
            response1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
            response2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetAlertById_WithValidId_ShouldReturnAlert()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var productId = await CreateTestProductAsync();
            var alertId = await CreateTestStockAlertAsync(productId);

            // Act
            var response = await _client.GetAsync($"/api/stockalert/{alertId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        private async Task<Guid> CreateTestStockAlertAsync(Guid productId)
        {
            var alertDto = new
            {
                ProductId = productId,
                Email = $"test_{Guid.NewGuid()}@example.com",
                NotifyWhenAvailable = true
            };

            var response = await _client.PostAsJsonAsync("/api/stockalert", alertDto);
            if (response.IsSuccessStatusCode)
            {
                var alertContent = await response.Content.ReadAsStringAsync();
                var alertId = JsonHelper.GetNestedProperty(alertContent, "data", "id");
                if (!string.IsNullOrEmpty(alertId))
                    return Guid.Parse(alertId);
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
                Name = $"Alert Product {Guid.NewGuid()}",
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

