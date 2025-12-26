namespace OnlineShop.Domain.Interfaces.Services
{
    /// <summary>
    /// Interface for payment gateway services
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Request payment from gateway and get payment URL
        /// </summary>
        /// <param name="request">Payment request details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment request result with token and payment URL</returns>
        Task<PaymentRequestResult> RequestPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify payment transaction with gateway
        /// </summary>
        /// <param name="transactionId">Transaction ID (Token) from gateway</param>
        /// <param name="amount">Payment amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Payment verification result</returns>
        Task<PaymentVerifyResult> VerifyPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refund a payment transaction
        /// </summary>
        /// <param name="transactionId">Transaction ID to refund</param>
        /// <param name="amount">Refund amount</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refund result</returns>
        Task<PaymentRefundResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Payment request model
    /// </summary>
    public class PaymentRequest
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? UserEmail { get; set; }
        public string? UserMobile { get; set; }
    }

    /// <summary>
    /// Payment request result
    /// </summary>
    public class PaymentRequestResult
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? PaymentUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
    }

    /// <summary>
    /// Payment verification result
    /// </summary>
    public class PaymentVerifyResult
    {
        public bool IsSuccess { get; set; }
        public bool IsVerified { get; set; }
        public string? TransactionId { get; set; }
        public string? RetrievalRefNo { get; set; }
        public string? SystemTraceNo { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
    }

    /// <summary>
    /// Payment refund result
    /// </summary>
    public class PaymentRefundResult
    {
        public bool IsSuccess { get; set; }
        public string? RefundId { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ErrorCode { get; set; }
    }
}

