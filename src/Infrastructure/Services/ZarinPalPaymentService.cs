using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts.Services;
using System.Text;
using System.Text.Json;

namespace OnlineShop.Infrastructure.Services
{
    public class ZarinPalPaymentService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZarinPalPaymentService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _merchantId;
        private readonly bool _isSandbox;
        private readonly string _baseUrl;

        public ZarinPalPaymentService(
            IConfiguration configuration,
            ILogger<ZarinPalPaymentService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            
            _merchantId = _configuration["ZarinPal:MerchantId"] ?? throw new InvalidOperationException("ZarinPal MerchantId not configured");
            _baseUrl = "https://payment.zarinpal.com/pg/v4/payment";
            
            _logger.LogInformation("ZarinPal Payment Service initialized. Sandbox: {IsSandbox}", _isSandbox);
        }

        public async Task<(bool Success, string Url, string Authority, string Message)> InitiatePaymentAsync(
            Guid orderId, 
            long amount, 
            string mobile, 
            string description)
        {
            try
            {
                var callbackUrl = _configuration["ZarinPal:CallbackUrl"] ?? "https://localhost:5001/api/Payment/verify";
                
                var requestData = new
                {
                    merchant_id = _merchantId,
                    amount = amount,
                    currency = "IRT", // تومان
                    callback_url = callbackUrl,
                    description = description,
                    metadata = new
                    {
                        mobile = mobile,
                        order_id = orderId.ToString()
                    }
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Initiating ZarinPal payment. OrderId: {OrderId}, Amount: {Amount}", orderId, amount);

                var response = await _httpClient.PostAsync($"{_baseUrl}/request.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("ZarinPal Response: {Response}", responseContent);

                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;
                
                if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
                {
                    if (dataElement.TryGetProperty("code", out var codeElement) && codeElement.GetInt32() == 100)
                    {
                        var authority = dataElement.GetProperty("authority").GetString() ?? string.Empty;
                        var paymentUrl = $"https://payment.zarinpal.com/pg/StartPay/{authority}";

                        _logger.LogInformation("Payment initiated successfully. Authority: {Authority}", authority);
                        return (true, paymentUrl, authority, "Success");
                    }
                }
                
                var errorMessage = "خطای نامشخص";
                int errorCode = -999;
                
                if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
                {
                    if (errorsElement.TryGetProperty("message", out var msgElement) && msgElement.ValueKind == JsonValueKind.String)
                        errorMessage = msgElement.GetString() ?? errorMessage;
                        
                    if (errorsElement.TryGetProperty("code", out var errCodeElement) && errCodeElement.ValueKind == JsonValueKind.Number)
                        errorCode = errCodeElement.GetInt32();
                }

                _logger.LogWarning("Payment initiation failed. Code: {Code}, Message: {Message}", errorCode, errorMessage);
                return (false, string.Empty, string.Empty, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during payment initiation for OrderId: {OrderId}", orderId);
                return (false, string.Empty, string.Empty, $"خطا در برقراری ارتباط با درگاه پرداخت: {ex.Message}");
            }
        }

        public async Task<(bool Success, string RefId, string Message)> VerifyPaymentAsync(string authority, long amount)
        {
            try
            {
                var requestData = new
                {
                    merchant_id = _merchantId,
                    amount = amount,
                    authority = authority
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Verifying ZarinPal payment. Authority: {Authority}, Amount: {Amount}", authority, amount);

                var response = await _httpClient.PostAsync($"{_baseUrl}/verify.json", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("ZarinPal Verify Response: {Response}", responseContent);

                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;
                
                if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
                {
                    if (dataElement.TryGetProperty("code", out var codeElement) && (codeElement.GetInt32() == 100 || codeElement.GetInt32() == 101))
                    {
                        var code = codeElement.GetInt32();
                        var refId = dataElement.GetProperty("ref_id").GetInt64().ToString();
                        var message = code == 100 ? "پرداخت با موفقیت انجام شد" : "پرداخت قبلاً تایید شده بود";
                        
                        _logger.LogInformation("Payment verified successfully. RefId: {RefId}, Code: {Code}", refId, code);
                        return (true, refId, message);
                    }
                }
                
                int errorCode = -999;
                if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
                {
                    if (errorsElement.TryGetProperty("code", out var errCodeElement) && errCodeElement.ValueKind == JsonValueKind.Number)
                        errorCode = errCodeElement.GetInt32();
                }
                
                var errorMessage = GetErrorMessage(errorCode);
                _logger.LogWarning("Payment verification failed. Code: {Code}, Message: {Message}", errorCode, errorMessage);
                return (false, string.Empty, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during payment verification for Authority: {Authority}", authority);
                return (false, string.Empty, "خطا در تایید پرداخت");
            }
        }

        private string GetErrorMessage(int code)
        {
            return code switch
            {
                -9 => "خطای اعتبارسنجی",
                -10 => "IP یا مرچنت کد نامعتبر",
                -11 => "مرچنت کد غیرفعال",
                -14 => "آدرس بازگشت با دامنه ثبت شده مغایرت دارد",
                -15 => "درگاه پرداخت به حالت تعلیق درآمده",
                -41 => "حداکثر مبلغ پرداختی 100 میلیون تومان است",
                -50 => "مبلغ پرداخت شده با مقدار مبلغ ارسالی متفاوت است",
                -51 => "پرداخت ناموفق",
                -52 => "خطای غیرمنتظره",
                -53 => "پرداخت متعلق به این مرچنت کد نیست",
                -54 => "اتوریتی نامعتبر است",
                100 => "پرداخت موفق",
                101 => "پرداخت قبلاً تایید شده",
                _ => $"خطای نامشخص (کد: {code})"
            };
        }
    }
}
