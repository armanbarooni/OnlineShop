using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Infrastructure.Services
{
    /// <summary>
    /// Kavenegar SMS service implementation
    /// https://kavenegar.com/rest.html
    /// </summary>
    public class KavenegarSmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _settings;
        private readonly ILogger<KavenegarSmsService> _logger;

        public KavenegarSmsService(
            HttpClient httpClient,
            IOptions<SmsSettings> settings,
            ILogger<KavenegarSmsService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                var apiUrl = $"https://api.kavenegar.com/v1/{_settings.ApiKey}/sms/send.json";
                
                var parameters = new Dictionary<string, string>
                {
                    { "receptor", phoneNumber },
                    { "message", message },
                    { "sender", _settings.Sender }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send SMS to {PhoneNumber}. Status: {StatusCode}, Error: {Error}", 
                    phoneNumber, response.StatusCode, errorContent);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
        {
            try
            {
                // Use Kavenegar's lookup/verify endpoint for OTP templates
                var apiUrl = $"https://api.kavenegar.com/v1/{_settings.ApiKey}/verify/lookup.json";
                
                var parameters = new Dictionary<string, string>
                {
                    { "receptor", phoneNumber },
                    { "token", code },
                    { "template", _settings.OtpTemplate }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("OTP sent successfully to {PhoneNumber}", phoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send OTP to {PhoneNumber}. Status: {StatusCode}, Error: {Error}", 
                    phoneNumber, response.StatusCode, errorContent);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending OTP to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}

