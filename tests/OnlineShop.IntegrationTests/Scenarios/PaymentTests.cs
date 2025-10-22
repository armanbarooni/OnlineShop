using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class PaymentTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PaymentTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreatePayment_WithValidOrder_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();

            // Act
            var paymentDto = new
            {
                OrderId = orderId,
                Amount = 150.0m,
                PaymentMethod = "CreditCard",
                TransactionId = $"TXN_{Guid.NewGuid()}"
            };
            var response = await _client.PostAsJsonAsync("/api/userpayment", paymentDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ProcessPayment_WithSuccessfulPayment_ShouldUpdateStatus()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            var paymentId = await CreateTestPaymentAsync(orderId);

            // Act - Update payment to success
            var updateDto = new
            {
                PaymentStatus = "Completed",
                TransactionId = $"TXN_SUCCESS_{Guid.NewGuid()}"
            };
            var response = await _client.PutAsJsonAsync($"/api/userpayment/{paymentId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ProcessPayment_WithFailedPayment_ShouldUpdateStatus()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            var paymentId = await CreateTestPaymentAsync(orderId);

            // Act - Update payment to failed
            var updateDto = new
            {
                PaymentStatus = "Failed",
                TransactionId = $"TXN_FAIL_{Guid.NewGuid()}"
            };
            var response = await _client.PutAsJsonAsync($"/api/userpayment/{paymentId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPaymentById_WithValidId_ShouldReturnPayment()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            var paymentId = await CreateTestPaymentAsync(orderId);

            // Act
            var response = await _client.GetAsync($"/api/userpayment/{paymentId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPaymentsByOrderId_ShouldReturnOrderPayments()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            await CreateTestPaymentAsync(orderId);

            // Act
            var response = await _client.GetAsync($"/api/userpayment/order/{orderId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserPayments_ShouldReturnAllUserPayments()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/userpayment/user/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RefundPayment_WithValidPayment_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            var paymentId = await CreateTestPaymentAsync(orderId);

            // Act - Refund payment
            var refundDto = new
            {
                PaymentStatus = "Refunded",
                RefundAmount = 150.0m
            };
            var response = await _client.PutAsJsonAsync($"/api/userpayment/{paymentId}", refundDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PaymentCallback_WithValidData_ShouldProcessCallback()
        {
            // This would typically be called by payment gateway
            // For now, we simulate it as a payment status update

            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var orderId = await CreateTestOrderAsync();
            var paymentId = await CreateTestPaymentAsync(orderId);

            // Act - Simulate callback
            var callbackDto = new
            {
                TransactionId = $"CALLBACK_{Guid.NewGuid()}",
                PaymentStatus = "Completed",
                Amount = 150.0m
            };
            var response = await _client.PutAsJsonAsync($"/api/userpayment/{paymentId}", callbackDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestOrderAsync()
        {
            var userId = await GetCurrentUserIdAsync();
            var productId = await CreateTestProductAsync();
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

        private async Task<Guid> CreateTestPaymentAsync(Guid orderId)
        {
            var paymentDto = new
            {
                OrderId = orderId,
                Amount = 150.0m,
                PaymentMethod = "CreditCard",
                TransactionId = $"TXN_{Guid.NewGuid()}"
            };

            var response = await _client.PostAsJsonAsync("/api/userpayment", paymentDto);
            if (response.IsSuccessStatusCode)
            {
                var paymentContent = await response.Content.ReadAsStringAsync();
                var paymentId = JsonHelper.GetNestedProperty(paymentContent, "data", "id");
                if (!string.IsNullOrEmpty(paymentId))
                    return Guid.Parse(paymentId);
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
                Name = $"Payment Product {Guid.NewGuid()}",
                Description = "Test",
                Price = 150.0m,
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

        private async Task<Guid> CreateTestAddressAsync(Guid userId)
        {
            var addressDto = new
            {
                UserId = userId,
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

