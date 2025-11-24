using System.Text.Json.Serialization;

namespace OnlineShop.Infrastructure.PaymentGateways.Sadad.Models
{
    /// <summary>
    /// Request model for Sadad payment gateway
    /// </summary>
    public class SadadPaymentRequest
    {
        [JsonPropertyName("MerchantId")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("TerminalId")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public long Amount { get; set; }

        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("LocalDateTime")]
        public string LocalDateTime { get; set; } = string.Empty;

        [JsonPropertyName("ReturnUrl")]
        public string ReturnUrl { get; set; } = string.Empty;

        [JsonPropertyName("SignData")]
        public string SignData { get; set; } = string.Empty;

        [JsonPropertyName("AdditionalData")]
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// Response model from Sadad payment gateway
    /// </summary>
    public class SadadPaymentResponse
    {
        [JsonPropertyName("ResCode")]
        public int ResCode { get; set; }

        [JsonPropertyName("Token")]
        public string? Token { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request model for Sadad payment verification
    /// </summary>
    public class SadadVerifyRequest
    {
        [JsonPropertyName("Token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("SignData")]
        public string SignData { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model from Sadad payment verification
    /// </summary>
    public class SadadVerifyResponse
    {
        [JsonPropertyName("ResCode")]
        public int ResCode { get; set; }

        [JsonPropertyName("Amount")]
        public long Amount { get; set; }

        [JsonPropertyName("RetrivalRefNo")]
        public string? RetrivalRefNo { get; set; }

        [JsonPropertyName("SystemTraceNo")]
        public string? SystemTraceNo { get; set; }

        [JsonPropertyName("OrderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Request model for Sadad payment refund
    /// </summary>
    public class SadadRefundRequest
    {
        [JsonPropertyName("TerminalId")]
        public string TerminalId { get; set; } = string.Empty;

        [JsonPropertyName("RetrivalRefNo")]
        public string RetrivalRefNo { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public long Amount { get; set; }

        [JsonPropertyName("SignData")]
        public string SignData { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model from Sadad payment refund
    /// </summary>
    public class SadadRefundResponse
    {
        [JsonPropertyName("ResCode")]
        public int ResCode { get; set; }

        [JsonPropertyName("RefundId")]
        public string? RefundId { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Sadad error codes mapping
    /// </summary>
    public static class SadadErrorCodes
    {
        public const int Success = 0;
        public const int InvalidMerchant = 1;
        public const int InvalidTerminal = 2;
        public const int InvalidAmount = 3;
        public const int InvalidOrderId = 4;
        public const int InvalidSignData = 5;
        public const int TransactionNotFound = 6;
        public const int TransactionAlreadyVerified = 7;
        public const int InsufficientFunds = 8;
        public const int TransactionExpired = 9;
        public const int SystemError = 10;

        public static string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                Success => "عملیات با موفقیت انجام شد",
                InvalidMerchant => "شناسه پذیرنده نامعتبر است",
                InvalidTerminal => "شناسه ترمینال نامعتبر است",
                InvalidAmount => "مبلغ نامعتبر است",
                InvalidOrderId => "شناسه سفارش نامعتبر است",
                InvalidSignData => "امضای دیجیتال نامعتبر است",
                TransactionNotFound => "تراکنش یافت نشد",
                TransactionAlreadyVerified => "تراکنش قبلاً تایید شده است",
                InsufficientFunds => "موجودی کافی نیست",
                TransactionExpired => "تراکنش منقضی شده است",
                SystemError => "خطای سیستم",
                _ => $"خطای نامشخص با کد: {errorCode}"
            };
        }
    }
}

