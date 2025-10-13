using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;

namespace OnlineShop.Infrastructure.Services
{
    /// <summary>
    /// Mock SMS service for development and testing
    /// Logs SMS messages instead of sending them
    /// </summary>
    public class MockSmsService : ISmsService
    {
        private readonly ILogger<MockSmsService> _logger;

        public MockSmsService(ILogger<MockSmsService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[MOCK SMS] To: {PhoneNumber} | Message: {Message}", 
                phoneNumber, 
                message);

            // Simulate successful send
            return Task.FromResult(true);
        }

        public Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[MOCK OTP] To: {PhoneNumber} | OTP Code: {Code}", 
                phoneNumber, 
                code);

            // Simulate successful send
            return Task.FromResult(true);
        }
    }
}

