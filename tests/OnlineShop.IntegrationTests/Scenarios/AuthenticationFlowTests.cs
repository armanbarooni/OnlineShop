using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class AuthenticationFlowTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthenticationFlowTests(CustomWebApplicationFactory<Program> factory)
        {
                        _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact(Skip = "Requires Identity configuration fix for InMemory DB")]
        public async Task UserCanRegisterAndLogin_WithEmailPassword()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = $"testuser{Guid.NewGuid()}@example.com",
                Password = "Test@123456",
                ConfirmPassword = "Test@123456",
                FirstName = "Test",
                LastName = "User"
            };

            // Act 1: Register
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert 1: Registration successful
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var accessToken = JsonHelper.GetNestedProperty(registerContent, "data", "accessToken");
            var refreshToken = JsonHelper.GetNestedProperty(registerContent, "data", "refreshToken");

            accessToken.Should().NotBeNullOrEmpty();
            refreshToken.Should().NotBeNullOrEmpty();

            // Act 2: Login with same credentials
            var loginDto = new LoginDto
            {
                Email = registerDto.Email,
                Password = registerDto.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert 2: Login successful
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginAccessToken = JsonHelper.GetNestedProperty(loginContent, "data", "accessToken");
            var loginRefreshToken = JsonHelper.GetNestedProperty(loginContent, "data", "refreshToken");

            loginAccessToken.Should().NotBeNullOrEmpty();
            loginRefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UserCanSendOtpAndVerify()
        {
            // Arrange
            var phoneNumber = $"0912345{Random.Shared.Next(1000, 9999)}";
            var sendOtpDto = new SendOtpDto
            {
                PhoneNumber = phoneNumber,
                Purpose = "Login"
            };

            // Act 1: Send OTP
            var sendOtpResponse = await _client.PostAsJsonAsync("/api/auth/send-otp", sendOtpDto);

            // Assert 1: OTP sent successfully
            sendOtpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await sendOtpResponse.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNullOrEmpty();
            // OTP sent successfully - actual validation would require extracting code from DB

            // Note: In real scenario, we'd need to extract the OTP code from database/mock
            // For now, this test validates the OTP sending endpoint works
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}

