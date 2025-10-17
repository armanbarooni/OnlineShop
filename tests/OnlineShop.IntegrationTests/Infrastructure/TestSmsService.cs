using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;

namespace OnlineShop.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Test SMS service that captures OTP codes for testing
    /// </summary>
    public class TestSmsService : ISmsService
    {
        private readonly ILogger<TestSmsService> _logger;
        private static readonly Dictionary<string, string> _otpCodes = new();

        public TestSmsService(ILogger<TestSmsService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[TEST SMS] To: {PhoneNumber} | Message: {Message}", 
                phoneNumber, 
                message);

            return Task.FromResult(true);
        }

        public Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[TEST OTP] To: {PhoneNumber} | Code: {Code}", 
                phoneNumber, 
                code);

            // Store the OTP code for this phone number
            _otpCodes[phoneNumber] = code;

            return Task.FromResult(true);
        }

        /// <summary>
        /// Get the last OTP code sent to a phone number
        /// </summary>
        public static string? GetLastOtpCode(string phoneNumber)
        {
            return _otpCodes.TryGetValue(phoneNumber, out var code) ? code : null;
        }

        /// <summary>
        /// Clear all stored OTP codes
        /// </summary>
        public static void ClearOtpCodes()
        {
            _otpCodes.Clear();
        }
    }
}

