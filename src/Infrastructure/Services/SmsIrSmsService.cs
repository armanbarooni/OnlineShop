using IPE.SmsIrClient;
using IPE.SmsIrClient.Models.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.Settings;

namespace OnlineShop.Infrastructure.Services;

public class SmsIrSmsService : ISmsService
{
    private readonly ILogger<SmsIrSmsService> _logger;
    private readonly SmsIrSettings _options;

    public SmsIrSmsService(IOptions<SmsIrSettings> options, ILogger<SmsIrSmsService> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("SmsIr ApiKey is not configured. Skipping SendSmsAsync.");
            return false;
        }

        try
        {
            var client = new SmsIr(_options.ApiKey);
            // BulkSend requires a lineNumber; for OTP we prefer VerifySend.
            var result = await client.BulkSendAsync(95000000000000, message, new[] { Normalize(phoneNumber) });
            var ok = result?.Status == 1;
            _logger.LogInformation("SmsIr BulkSend status={Status} message={Msg}", result?.Status, result?.Message);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SmsIr BulkSend failed");
            return false;
        }
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("SmsIr ApiKey is not configured. Skipping SendOtpAsync.");
            return false;
        }

        var mobile = Normalize(phoneNumber);
        var templateId = _options.TemplateId <= 0 ? 123456 : _options.TemplateId;

        // minimal retry for 429/500
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var client = new SmsIr(_options.ApiKey);
                var paramName = string.IsNullOrWhiteSpace(_options.OtpParamName) ? "Code" : _options.OtpParamName;
                var parameters = new[] { new VerifySendParameter(paramName, code) };
                var result = await client.VerifySendAsync(mobile, templateId, parameters);
                var ok = result?.Status == 1;
                _logger.LogInformation("SmsIr VerifySend status={Status} message={Msg} messageId={Id}", result?.Status, result?.Message, result?.Data?.MessageId);
                if (ok) return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SmsIr VerifySend attempt {Attempt} failed", attempt);
            }
            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
        }

        return false;
    }

    private static string Normalize(string phone)
    {
        // convert 09xxxxxxxxx to 9xxxxxxxxx for SMSIR if needed
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        return phone.StartsWith("0") ? phone.Substring(1) : phone;
    }
}


