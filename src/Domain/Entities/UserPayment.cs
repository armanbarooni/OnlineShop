using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserPayment : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid? OrderId { get; private set; }
        public string PaymentMethod { get; private set; } = string.Empty;
        public string PaymentStatus { get; private set; } = string.Empty;
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "IRR";
        public string? TransactionId { get; private set; }
        public string? GatewayResponse { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public string? FailureReason { get; private set; }
        public string? RefundId { get; private set; }
        public decimal? RefundAmount { get; private set; }
        public DateTime? RefundedAt { get; private set; }
        public string? RefundReason { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual UserOrder? Order { get; private set; }

        protected UserPayment() { }

        private UserPayment(Guid userId, Guid? orderId, string paymentMethod, decimal amount, string currency)
        {
            UserId = userId;
            OrderId = orderId;
            SetPaymentMethod(paymentMethod);
            SetAmount(amount);
            SetCurrency(currency);
            PaymentStatus = "Pending";
            Deleted = false;
        }

        public static UserPayment Create(Guid userId, Guid? orderId, string paymentMethod, decimal amount, string currency = "IRR")
            => new(userId, orderId, paymentMethod, amount, currency);

        public void SetPaymentMethod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("روش پرداخت نباید خالی باشد");
            PaymentMethod = paymentMethod.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("مبلغ پرداخت باید بزرگتر از صفر باشد");
            Amount = amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("واحد پول نباید خالی باشد");
            Currency = currency.Trim().ToUpper();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetTransactionId(string? transactionId)
        {
            TransactionId = transactionId?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetGatewayResponse(string? gatewayResponse)
        {
            GatewayResponse = gatewayResponse?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessing(string? gatewayTransactionId = null)
        {
            if (PaymentStatus != "Pending")
                throw new InvalidOperationException($"پرداخت در وضعیت {PaymentStatus} است و نمی‌تواند پردازش شود");
            
            PaymentStatus = "Processing";
            SetTransactionId(gatewayTransactionId);
            SetGatewayResponse("Payment is being processed");
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsPaid(string? transactionId, string? gatewayResponse)
        {
            PaymentStatus = "Paid";
            SetTransactionId(transactionId);
            SetGatewayResponse(gatewayResponse);
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? failureReason)
        {
            PaymentStatus = "Failed";
            FailureReason = failureReason?.Trim();
            FailedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRefunded(decimal refundAmount, string? refundId, string? refundReason)
        {
            PaymentStatus = "Refunded";
            RefundAmount = refundAmount;
            RefundId = refundId?.Trim();
            RefundReason = refundReason?.Trim();
            RefundedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string paymentMethod, decimal amount, string currency, string? updatedBy)
        {
            SetPaymentMethod(paymentMethod);
            SetAmount(amount);
            SetCurrency(currency);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این پرداخت قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
