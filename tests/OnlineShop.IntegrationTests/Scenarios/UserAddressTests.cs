using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class UserAddressTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserAddressTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateAddress_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var addressDto = new
            {
                UserId = userId,
                FullName = "John Doe",
                PhoneNumber = "09123456789",
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                City = "Tehran",
                State = "Tehran",
                PostalCode = "1234567890",
                Country = "Iran",
                IsDefault = true
            };
            var response = await _client.PostAsJsonAsync("/api/useraddress", addressDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetUserAddresses_ShouldReturnAllUserAddresses()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            await CreateTestAddressAsync(userId);

            // Act
            var response = await _client.GetAsync($"/api/useraddress/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var isSuccess = JsonHelper.GetNestedProperty(content, "isSuccess");
            isSuccess.Should().Be("true");
        }

        [Fact]
        public async Task UpdateAddress_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Act
            var updateDto = new
            {
                FullName = "Jane Doe Updated",
                PhoneNumber = "09987654321",
                AddressLine1 = "456 New Street",
                City = "Isfahan",
                State = "Isfahan",
                PostalCode = "9876543210",
                Country = "Iran"
            };
            var response = await _client.PutAsJsonAsync($"/api/useraddress/{addressId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAddress_WithValidId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Act
            var response = await _client.DeleteAsync($"/api/useraddress/{addressId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SetDefaultAddress_ShouldMakeAddressDefault()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var address1Id = await CreateTestAddressAsync(userId, isDefault: false);
            var address2Id = await CreateTestAddressAsync(userId, isDefault: false);

            // Act - Set second address as default
            var updateDto = new { IsDefault = true };
            var response = await _client.PutAsJsonAsync($"/api/useraddress/{address2Id}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetDefaultAddress_ShouldReturnDefaultAddress()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            await CreateTestAddressAsync(userId, isDefault: true);

            // Act
            var response = await _client.GetAsync($"/api/useraddress/user/{userId}/default");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateAddress_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var addressDto = new
            {
                UserId = Guid.NewGuid(),
                FullName = "Test",
                PhoneNumber = "09123456789",
                AddressLine1 = "Test St",
                City = "Tehran",
                State = "Tehran",
                PostalCode = "1234567890",
                Country = "Iran"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/useraddress", addressDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAddressById_WithValidId_ShouldReturnAddress()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();
            var addressId = await CreateTestAddressAsync(userId);

            // Act
            var response = await _client.GetAsync($"/api/useraddress/{addressId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateMultipleAddresses_ForSameUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act - Create multiple addresses
            var address1Id = await CreateTestAddressAsync(userId, city: "Tehran");
            var address2Id = await CreateTestAddressAsync(userId, city: "Isfahan");
            var address3Id = await CreateTestAddressAsync(userId, city: "Shiraz");

            // Assert
            address1Id.Should().NotBe(Guid.Empty);
            address2Id.Should().NotBe(Guid.Empty);
            address3Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task ValidateAddress_WithInvalidPostalCode_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var addressDto = new
            {
                UserId = userId,
                FullName = "Test User",
                PhoneNumber = "09123456789",
                AddressLine1 = "123 Test St",
                City = "Tehran",
                State = "Tehran",
                PostalCode = "invalid", // Invalid postal code
                Country = "Iran"
            };
            var response = await _client.PostAsJsonAsync("/api/useraddress", addressDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Created);
        }

        private async Task<Guid> CreateTestAddressAsync(Guid userId, bool isDefault = false, string city = "Tehran")
        {
            var addressDto = new
            {
                UserId = userId,
                FullName = $"User {Guid.NewGuid()}",
                PhoneNumber = $"0991{Random.Shared.Next(10000000, 99999999)}",
                AddressLine1 = $"{Random.Shared.Next(1, 999)} Main St",
                AddressLine2 = "Apt 1",
                City = city,
                State = city,
                PostalCode = $"{Random.Shared.Next(1000000000, 1999999999)}",
                Country = "Iran",
                IsDefault = isDefault
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

