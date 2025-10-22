using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Test SMS service that captures OTP codes for testing
    /// </summary>
    public class TestSmsService : ISmsService
    {
        private readonly ILogger<TestSmsService> _logger;
        private readonly IOtpRepository _otpRepository;
        private static readonly Dictionary<string, string> _otpCodes = new();

        public TestSmsService(ILogger<TestSmsService> logger, IOtpRepository otpRepository)
        {
            _logger = logger;
            _otpRepository = otpRepository;
        }

        public Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[TEST SMS] To: {PhoneNumber} | Message: {Message}", 
                phoneNumber, 
                message);

            return Task.FromResult(true);
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[TEST OTP] To: {PhoneNumber} | Code: {Code}", 
                phoneNumber, 
                code);

            // Store the OTP code for this phone number in static dictionary
            _otpCodes[phoneNumber] = code;

            // Also create/update OTP in database for validation
            try
            {
                var existingOtp = await _otpRepository.GetValidOtpByPhoneAsync(phoneNumber, cancellationToken);
                if (existingOtp != null)
                {
                    existingOtp.SetCode(code);
                    await _otpRepository.UpdateAsync(existingOtp, cancellationToken);
                }
                else
                {
                    // Create new OTP if none exists
                    var newOtp = Otp.Create(phoneNumber, code, 10, "login");
                    await _otpRepository.AddAsync(newOtp, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to update OTP in database: {Error}", ex.Message);
            }

            return Task.FromResult(true).Result;
        }

        /// <summary>
        /// Get the last OTP code sent to a phone number
        /// </summary>
        public static string? GetLastOtpCode(string phoneNumber)
        {
            return _otpCodes.TryGetValue(phoneNumber, out var code) ? code : null;
        }

        /// <summary>
        /// Get the last OTP code from database for a phone number
        /// </summary>
        public async Task<string?> GetLastOtpCodeFromDatabaseAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(phoneNumber, cancellationToken);
            return otp?.Code;
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

