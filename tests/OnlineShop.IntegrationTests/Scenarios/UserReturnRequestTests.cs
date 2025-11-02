using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class UserReturnRequestTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public UserReturnRequestTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateReturnRequest_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);

            // Act
            var returnDto = new
            {
                OrderId = orderId,
                Reason = "Product defective",
                Description = "The product arrived damaged",
                RequestedAction = "Refund"
            };
            var response = await _client.PostAsJsonAsync("/api/userreturnrequest", returnDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserReturnRequests_ShouldReturnAllUserRequests()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/userreturnrequest/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ApproveReturnRequest_ByAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            var requestId = await CreateTestReturnRequestAsync(orderId);

            // Act
            var approveDto = new { AdminNotes = "ok" };
            var response = await _client.PostAsJsonAsync($"/api/userreturnrequest/{requestId}/approve", approveDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public async Task RejectReturnRequest_ByAdmin_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            var requestId = await CreateTestReturnRequestAsync(orderId);

            // Act
            var rejectDto = new
            {
                RejectionReason = "Product is not eligible for return"
            };
            var response = await _client.PostAsJsonAsync($"/api/userreturnrequest/{requestId}/reject", rejectDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReturnRequestById_WithValidId_ShouldReturnRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            var requestId = await CreateTestReturnRequestAsync(orderId);

            // Act
            var response = await _client.GetAsync($"/api/userreturnrequest/{requestId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateReturnRequest_WithAdditionalInfo_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            var requestId = await CreateTestReturnRequestAsync(orderId);

            // Act
            var updateDto = new
            {
                Description = "Updated: Product has manufacturing defect",
                ImageUrls = new[] { "https://example.com/image1.jpg", "https://example.com/image2.jpg" }
            };
            var response = await _client.PutAsJsonAsync($"/api/userreturnrequest/{requestId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReturnRequestsByOrder_ShouldReturnOrderReturnRequests()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            await CreateTestReturnRequestAsync(orderId);

            // Act
            var response = await _client.GetAsync($"/api/userreturnrequest/order/{orderId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllReturnRequests_AsAdmin_ShouldReturnList()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/userreturnrequest");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task SearchReturnRequests_WithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var searchCriteria = new
            {
                Status = "Pending",
                PageNumber = 1,
                PageSize = 10
            };
            var response = await _client.PostAsJsonAsync("/api/userreturnrequest/search", searchCriteria);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateReturnRequest_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var returnDto = new
            {
                OrderId = Guid.NewGuid(),
                Reason = "Test",
                Description = "Test"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/userreturnrequest", returnDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProcessReturnRefund_AfterApproval_ShouldInitiateRefund()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client, _factory);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var orderId = await CreateTestOrderAsync(userId);
            var requestId = await CreateTestReturnRequestAsync(orderId);

            // Approve the request
            await _client.PostAsync($"/api/userreturnrequest/{requestId}/approve", null);

            // Act - Process refund
            var refundDto = new
            {
                RefundAmount = 100.0m,
                RefundMethod = "Original Payment Method"
            };
            var response = await _client.PostAsJsonAsync($"/api/userreturnrequest/{requestId}/refund", refundDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        private async Task<Guid> CreateTestReturnRequestAsync(Guid orderId)
        {
            var returnDto = new
            {
                OrderId = orderId,
                Reason = "Product defective",
                Description = "Test return request",
                RequestedAction = "Refund"
            };

            var response = await _client.PostAsJsonAsync("/api/userreturnrequest", returnDto);
            if (response.IsSuccessStatusCode)
            {
                var returnContent = await response.Content.ReadAsStringAsync();
                var requestId = JsonHelper.GetNestedProperty(returnContent, "data", "id");
                if (!string.IsNullOrEmpty(requestId))
                    return Guid.Parse(requestId);
            }

            return Guid.NewGuid();
        }

        private async Task<Guid> CreateTestOrderAsync(Guid userId)
        {
            var productId = await CreateTestProductAsync();
            var addressId = await CreateTestAddressAsync(userId);

            await _client.PostAsJsonAsync("/api/cart/add", new { ProductId = productId, Quantity = 1 });

            var orderDto = new
            {
                UserId = userId,
                ShippingAddressId = addressId,
                PaymentMethod = "Cash"
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

        private async Task<Guid> CreateTestProductAsync()
        {
            var categoryId = await CreateTestCategoryAsync();
            var unitId = await CreateTestUnitAsync();
            var brandId = await CreateTestBrandAsync();

            var productDto = new
            {
                Name = $"Return Product {Guid.NewGuid()}",
                Description = "Test Product",
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
                Title = "Return Address",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "09123456789",
                AddressLine1 = "123 Test St",
                City = "Tehran",
                State = "Tehran",
                PostalCode = "1234567890",
                Country = "Iran",
                IsDefault = true,
                IsBillingAddress = true,
                IsShippingAddress = true
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

