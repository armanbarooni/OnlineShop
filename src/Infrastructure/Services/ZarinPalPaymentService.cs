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
            _isSandbox = _configuration.GetValue<bool>("ZarinPal:IsSandbox", true);
            _baseUrl = _isSandbox 
                ? "https://sandbox.zarinpal.com/pg/v4/payment"
                : "https://payment.zarinpal.com/pg/v4/payment";
            
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

                var result = JsonSerializer.Deserialize<ZarinPalResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.Data?.Code == 100)
                {
                    var authority = result.Data.Authority;
                    var paymentUrl = _isSandbox
                        ? $"https://sandbox.zarinpal.com/pg/StartPay/{authority}"
                        : $"https://payment.zarinpal.com/pg/StartPay/{authority}";

                    _logger.LogInformation("Payment initiated successfully. Authority: {Authority}", authority);
                    return (true, paymentUrl, authority, "Success");
                }
                else
                {
                    var errorMessage = result?.Data?.Message ?? "خطای نامشخص";
                    _logger.LogWarning("Payment initiation failed. Code: {Code}, Message: {Message}", 
                        result?.Data?.Code, errorMessage);
                    return (false, string.Empty, string.Empty, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during payment initiation for OrderId: {OrderId}", orderId);
                return (false, string.Empty, string.Empty, "خطا در برقراری ارتباط با درگاه پرداخت");
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

                var result = JsonSerializer.Deserialize<ZarinPalVerifyResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.Data?.Code == 100 || result?.Data?.Code == 101)
                {
                    var refId = result.Data.RefId?.ToString() ?? string.Empty;
                    var message = result.Data.Code == 100 ? "پرداخت با موفقیت انجام شد" : "پرداخت قبلاً تایید شده بود";
                    
                    _logger.LogInformation("Payment verified successfully. RefId: {RefId}, Code: {Code}", refId, result.Data.Code);
                    return (true, refId, message);
                }
                else
                {
                    var errorMessage = GetErrorMessage(result?.Data?.Code ?? -999);
                    _logger.LogWarning("Payment verification failed. Code: {Code}, Message: {Message}", 
                        result?.Data?.Code, errorMessage);
                    return (false, string.Empty, errorMessage);
                }
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

        // Response Models
        private class ZarinPalResponse
        {
            public ZarinPalData? Data { get; set; }
            public List<object>? Errors { get; set; }
        }

        private class ZarinPalData
        {
            public int Code { get; set; }
            public string? Message { get; set; }
            public string? Authority { get; set; }
            public string? FeeType { get; set; }
            public int Fee { get; set; }
        }

        private class ZarinPalVerifyResponse
        {
            public ZarinPalVerifyData? Data { get; set; }
            public List<object>? Errors { get; set; }
        }

        private class ZarinPalVerifyData
        {
            public int Code { get; set; }
            public string? Message { get; set; }
            public long? RefId { get; set; }
            public string? CardHash { get; set; }
            public string? CardPan { get; set; }
            public string? FeeType { get; set; }
            public int Fee { get; set; }
        }
    }
}
