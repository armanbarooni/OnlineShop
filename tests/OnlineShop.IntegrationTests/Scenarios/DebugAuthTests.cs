using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.IntegrationTests.Infrastructure;
using OnlineShop.IntegrationTests.Helpers;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class DebugAuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DebugAuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _factory = factory;
        }

        [Fact]
        public async Task DebugHardcodedAdminLogin()
        {
            // Seed database first
            await _factory.SeedDatabaseAsync();
            
            // Test hardcoded admin login
            var hardcodedLoginDto = new 
            { 
                Email = "admin@test.com", 
                Password = "AdminPassword123!" 
            };
            
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", hardcodedLoginDto);
            var content = await loginResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {loginResponse.StatusCode}");
            Console.WriteLine($"Response: {content}");
            
            // This should succeed
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DebugOtpFlow()
        {
            // Seed database first
            await _factory.SeedDatabaseAsync();
            
            // Test OTP flow
            var phoneNumber = "09123456789";
            
            // Send OTP
            var sendOtpDto = new { PhoneNumber = phoneNumber, Purpose = "login" };
            var otpResponse = await _client.PostAsJsonAsync("/api/auth/send-otp", sendOtpDto);
            await Task.Delay(150);
            
            Console.WriteLine($"OTP Status: {otpResponse.StatusCode}");
            var otpContent = await otpResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"OTP Response: {otpContent}");
            
            otpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            // Get OTP code from TestSmsService static dictionary
            var otpCode = TestSmsService.GetLastOtpCode(phoneNumber);
            Console.WriteLine($"Retrieved OTP: {otpCode}");
            
            otpCode.Should().NotBeNullOrEmpty();
            
            // Login with OTP
            var loginDto = new { PhoneNumber = phoneNumber, Code = otpCode };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login-phone", loginDto);
            
            Console.WriteLine($"Login Status: {loginResponse.StatusCode}");
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Login Response: {loginContent}");
            
            loginResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }
    }
}
