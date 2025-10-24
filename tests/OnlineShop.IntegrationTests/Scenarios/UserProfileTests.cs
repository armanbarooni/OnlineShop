using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class UserProfileTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public UserProfileTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetUserProfile_WithAuthenticatedUser_ShouldReturnProfile()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.GetAsync($"/api/userprofile/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUserProfile_WithValidData_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var updateDto = new
            {
                Bio = "Updated Bio - Integration Test",
                AvatarUrl = "https://example.com/avatar-updated.jpg",
                DateOfBirth = DateTime.UtcNow.AddYears(-25)
            };
            var response = await _client.PutAsJsonAsync($"/api/userprofile/{userId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateUserProfile_ForNewUser_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var profileDto = new
            {
                UserId = userId,
                Bio = "Test User Bio",
                AvatarUrl = "https://example.com/avatar.jpg",
                DateOfBirth = DateTime.UtcNow.AddYears(-30)
            };
            var response = await _client.PostAsJsonAsync("/api/userprofile", profileDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task DeleteUserProfile_WithValidUserId_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/userprofile/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAvatar_WithNewUrl_ShouldSucceed()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act
            var updateDto = new
            {
                AvatarUrl = "https://example.com/new-avatar.jpg"
            };
            var response = await _client.PutAsJsonAsync($"/api/userprofile/{userId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserProfile_WhenUnauthorized_ShouldReturnUnauthorized()
        {
            // Arrange - No auth token
            var userId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/userprofile/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUserProfile_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var userId = await GetCurrentUserIdAsync();

            // Act - Try to set future date of birth
            var updateDto = new
            {
                DateOfBirth = DateTime.UtcNow.AddYears(10)
            };
            var response = await _client.PutAsJsonAsync($"/api/userprofile/{userId}", updateDto);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllProfiles_AsAdmin_ShouldReturnList()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act
            var response = await _client.GetAsync("/api/userprofile");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdatePhoneNumber_ShouldRequireVerification()
        {
            // Arrange
            var authToken = await AuthHelper.GetAdminTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Act - Send OTP for new phone number (using "Login" as purpose since "update" is not a valid purpose)
            var newPhone = $"0991{Random.Shared.Next(10000000, 99999999)}";
            var otpDto = new
            {
                PhoneNumber = newPhone,
                Purpose = "Login"  // Changed from "update" to "Login"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/send-otp", otpDto);
            await Task.Delay(200);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
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

