namespace OnlineShop.Infrastructure.PaymentGateways.Sadad.Configuration
{
    /// <summary>
    /// Configuration class for Sadad payment gateway
    /// </summary>
    public class SadadGatewayConfig
    {
        /// <summary>
        /// Merchant ID from Sadad
        /// </summary>
        public string MerchantId { get; set; } = string.Empty;

        /// <summary>
        /// Terminal ID from Sadad
        /// </summary>
        public string TerminalId { get; set; } = string.Empty;

        /// <summary>
        /// Security key for generating SignData
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for production environment
        /// </summary>
        public string BaseUrl { get; set; } = "https://sadad.shaparak.ir/api/v0";

        /// <summary>
        /// Base URL for sandbox environment
        /// </summary>
        public string SandboxUrl { get; set; } = "https://sandbox.sadad.shaparak.ir/api/v0";

        /// <summary>
        /// Use sandbox environment instead of production
        /// </summary>
        public bool UseSandbox { get; set; } = true;

        /// <summary>
        /// Callback URL for payment return
        /// </summary>
        public string CallbackUrl { get; set; } = string.Empty;

        /// <summary>
        /// Get the appropriate base URL based on UseSandbox setting
        /// </summary>
        public string GetBaseUrl() => UseSandbox ? SandboxUrl : BaseUrl;

        /// <summary>
        /// Payment request endpoint
        /// </summary>
        public string PaymentRequestEndpoint => "/Request/PaymentRequest";

        /// <summary>
        /// Payment verification endpoint
        /// </summary>
        public string PaymentVerifyEndpoint => "/Request/PaymentVerification";

        /// <summary>
        /// Payment refund endpoint
        /// </summary>
        public string PaymentRefundEndpoint => "/Request/RefundRequest";
    }
}

