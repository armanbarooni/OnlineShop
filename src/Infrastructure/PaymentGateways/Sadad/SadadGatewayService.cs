using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Domain.Interfaces.Services;
using OnlineShop.Infrastructure.PaymentGateways.Sadad.Configuration;
using OnlineShop.Infrastructure.PaymentGateways.Sadad.Exceptions;
using OnlineShop.Infrastructure.PaymentGateways.Sadad.Models;

namespace OnlineShop.Infrastructure.PaymentGateways.Sadad
{
    /// <summary>
    /// Implementation of IPaymentGateway for Sadad payment gateway
    /// </summary>
    public class SadadGatewayService : IPaymentGateway
    {
        private readonly SadadGatewayConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SadadGatewayService> _logger;

        public SadadGatewayService(
            IOptions<SadadGatewayConfig> config,
            IHttpClientFactory httpClientFactory,
            ILogger<SadadGatewayService> logger)
        {
            _config = config.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Request payment from Sadad gateway
        /// </summary>
        public async Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Requesting payment from Sadad gateway. PaymentId: {PaymentId}, Amount: {Amount}", 
                    request.PaymentId, request.Amount);

                // Convert amount to Rials (multiply by 10)
                var amountInRials = (long)(request.Amount * 10);

                // Generate SignData
                var signData = GenerateSignData(
                    _config.TerminalId,
                    _config.MerchantId,
                    request.OrderId.ToString(),
                    amountInRials,
                    _config.Key);

                // Create payment request
                var sadadRequest = new SadadPaymentRequest
                {
                    MerchantId = _config.MerchantId,
                    TerminalId = _config.TerminalId,
                    Amount = amountInRials,
                    OrderId = request.OrderId.ToString(),
                    LocalDateTime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReturnUrl = request.CallbackUrl,
                    SignData = signData,
                    AdditionalData = request.Description
                };

                // Send request to Sadad
                var httpClient = CreateHttpClient();
                var jsonContent = JsonSerializer.Serialize(sadadRequest, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_config.PaymentRequestEndpoint, content, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Sadad payment request response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sadad payment request failed with status: {StatusCode}", response.StatusCode);
                    return new PaymentRequestResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"خطا در ارتباط با درگاه پرداخت. کد خطا: {response.StatusCode}",
                        ErrorCode = (int)response.StatusCode
                    };
                }

                var sadadResponse = JsonSerializer.Deserialize<SadadPaymentResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (sadadResponse == null)
                {
                    _logger.LogError("Failed to deserialize Sadad response");
                    return new PaymentRequestResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "خطا در پردازش پاسخ درگاه پرداخت"
                    };
                }

                if (sadadResponse.ResCode != SadadErrorCodes.Success || string.IsNullOrEmpty(sadadResponse.Token))
                {
                    var errorMessage = SadadErrorCodes.GetErrorMessage(sadadResponse.ResCode);
                    _logger.LogWarning("Sadad payment request failed. ResCode: {ResCode}, Description: {Description}", 
                        sadadResponse.ResCode, sadadResponse.Description);
                    
                    return new PaymentRequestResult
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage,
                        ErrorCode = sadadResponse.ResCode
                    };
                }

                // Generate payment URL
                var paymentUrl = $"{_config.GetBaseUrl()}/Purchase/Index?token={sadadResponse.Token}";

                _logger.LogInformation("Sadad payment request successful. Token: {Token}", sadadResponse.Token);

                return new PaymentRequestResult
                {
                    IsSuccess = true,
                    Token = sadadResponse.Token,
                    PaymentUrl = paymentUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting payment from Sadad gateway");
                return new PaymentRequestResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"خطا در ارتباط با درگاه پرداخت: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Verify payment with Sadad gateway
        /// </summary>
        public async Task<PaymentVerifyResult> VerifyPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Verifying payment with Sadad gateway. Token: {Token}, Amount: {Amount}", 
                    transactionId, amount);

                // Convert amount to Rials
                var amountInRials = (long)(amount * 10);

                // Generate SignData for verification
                var signData = GenerateSignData(transactionId, _config.Key);

                // Create verify request
                var verifyRequest = new SadadVerifyRequest
                {
                    Token = transactionId,
                    SignData = signData
                };

                // Send request to Sadad
                var httpClient = CreateHttpClient();
                var jsonContent = JsonSerializer.Serialize(verifyRequest, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_config.PaymentVerifyEndpoint, content, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Sadad payment verification response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sadad payment verification failed with status: {StatusCode}", response.StatusCode);
                    return new PaymentVerifyResult
                    {
                        IsSuccess = false,
                        IsVerified = false,
                        ErrorMessage = $"خطا در ارتباط با درگاه پرداخت. کد خطا: {response.StatusCode}",
                        ErrorCode = (int)response.StatusCode
                    };
                }

                var verifyResponse = JsonSerializer.Deserialize<SadadVerifyResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (verifyResponse == null)
                {
                    _logger.LogError("Failed to deserialize Sadad verification response");
                    return new PaymentVerifyResult
                    {
                        IsSuccess = false,
                        IsVerified = false,
                        ErrorMessage = "خطا در پردازش پاسخ درگاه پرداخت"
                    };
                }

                if (verifyResponse.ResCode != SadadErrorCodes.Success)
                {
                    var errorMessage = SadadErrorCodes.GetErrorMessage(verifyResponse.ResCode);
                    _logger.LogWarning("Sadad payment verification failed. ResCode: {ResCode}, Description: {Description}", 
                        verifyResponse.ResCode, verifyResponse.Description);
                    
                    return new PaymentVerifyResult
                    {
                        IsSuccess = false,
                        IsVerified = false,
                        ErrorMessage = errorMessage,
                        ErrorCode = verifyResponse.ResCode
                    };
                }

                // Verify amount matches
                if (verifyResponse.Amount != amountInRials)
                {
                    _logger.LogWarning("Amount mismatch. Expected: {Expected}, Received: {Received}", 
                        amountInRials, verifyResponse.Amount);
                    return new PaymentVerifyResult
                    {
                        IsSuccess = false,
                        IsVerified = false,
                        ErrorMessage = "مبلغ پرداخت با مبلغ سفارش مطابقت ندارد"
                    };
                }

                _logger.LogInformation("Sadad payment verification successful. RetrievalRefNo: {RetrievalRefNo}", 
                    verifyResponse.RetrivalRefNo);

                return new PaymentVerifyResult
                {
                    IsSuccess = true,
                    IsVerified = true,
                    TransactionId = transactionId,
                    RetrievalRefNo = verifyResponse.RetrivalRefNo,
                    SystemTraceNo = verifyResponse.SystemTraceNo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment with Sadad gateway");
                return new PaymentVerifyResult
                {
                    IsSuccess = false,
                    IsVerified = false,
                    ErrorMessage = $"خطا در تایید پرداخت: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Refund payment through Sadad gateway
        /// </summary>
        public async Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Refunding payment through Sadad gateway. TransactionId: {TransactionId}, Amount: {Amount}", 
                    transactionId, amount);

                // Convert amount to Rials
                var amountInRials = (long)(amount * 10);

                // Generate SignData for refund
                var signData = GenerateSignData(
                    _config.TerminalId,
                    transactionId,
                    amountInRials.ToString(),
                    _config.Key);

                // Create refund request
                var refundRequest = new SadadRefundRequest
                {
                    TerminalId = _config.TerminalId,
                    RetrivalRefNo = transactionId,
                    Amount = amountInRials,
                    SignData = signData
                };

                // Send request to Sadad
                var httpClient = CreateHttpClient();
                var jsonContent = JsonSerializer.Serialize(refundRequest, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_config.PaymentRefundEndpoint, content, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Sadad payment refund response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sadad payment refund failed with status: {StatusCode}", response.StatusCode);
                    return new PaymentRefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"خطا در ارتباط با درگاه پرداخت. کد خطا: {response.StatusCode}",
                        ErrorCode = (int)response.StatusCode
                    };
                }

                var refundResponse = JsonSerializer.Deserialize<SadadRefundResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (refundResponse == null)
                {
                    _logger.LogError("Failed to deserialize Sadad refund response");
                    return new PaymentRefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "خطا در پردازش پاسخ درگاه پرداخت"
                    };
                }

                if (refundResponse.ResCode != SadadErrorCodes.Success)
                {
                    var errorMessage = SadadErrorCodes.GetErrorMessage(refundResponse.ResCode);
                    _logger.LogWarning("Sadad payment refund failed. ResCode: {ResCode}, Description: {Description}", 
                        refundResponse.ResCode, refundResponse.Description);
                    
                    return new PaymentRefundResult
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage,
                        ErrorCode = refundResponse.ResCode
                    };
                }

                _logger.LogInformation("Sadad payment refund successful. RefundId: {RefundId}", refundResponse.RefundId);

                return new PaymentRefundResult
                {
                    IsSuccess = true,
                    RefundId = refundResponse.RefundId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment through Sadad gateway");
                return new PaymentRefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"خطا در بازگشت وجه: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate SignData for payment request
        /// </summary>
        private string GenerateSignData(string terminalId, string merchantId, string orderId, long amount, string key)
        {
            var data = $"{terminalId};{merchantId};{orderId};{amount};{key}";
            return ComputeSha256Hash(data);
        }

        /// <summary>
        /// Generate SignData for payment verification
        /// </summary>
        private string GenerateSignData(string token, string key)
        {
            var data = $"{token};{key}";
            return ComputeSha256Hash(data);
        }

        /// <summary>
        /// Generate SignData for refund
        /// </summary>
        private string GenerateSignData(string terminalId, string retrievalRefNo, string amount, string key)
        {
            var data = $"{terminalId};{retrievalRefNo};{amount};{key}";
            return ComputeSha256Hash(data);
        }

        /// <summary>
        /// Compute SHA256 hash
        /// </summary>
        private string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Create HTTP client for Sadad API
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_config.GetBaseUrl());
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }
    }
}

