using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserReturnRequest : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid OrderItemId { get; private set; }
        public string ReturnReason { get; private set; } = string.Empty;
        public string ReturnStatus { get; private set; } = "Pending";
        public string? Description { get; private set; }
        public int Quantity { get; private set; }
        public decimal RefundAmount { get; private set; }
        public string? AdminNotes { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? ApprovedBy { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string? RejectedBy { get; private set; }
        public string? RejectionReason { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public string? ProcessedBy { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual UserOrder Order { get; private set; } = null!;
        public virtual UserOrderItem OrderItem { get; private set; } = null!;

        protected UserReturnRequest() { }

        private UserReturnRequest(Guid userId, Guid orderId, Guid orderItemId, string returnReason, 
            string? description, int quantity, decimal refundAmount)
        {
            UserId = userId;
            OrderId = orderId;
            OrderItemId = orderItemId;
            SetReturnReason(returnReason);
            SetDescription(description);
            SetQuantity(quantity);
            SetRefundAmount(refundAmount);
            ReturnStatus = "Pending";
            Deleted = false;
        }

        public static UserReturnRequest Create(Guid userId, Guid orderId, Guid orderItemId, 
            string returnReason, string? description, int quantity, decimal refundAmount)
            => new(userId, orderId, orderItemId, returnReason, description, quantity, refundAmount);

        public void SetReturnReason(string returnReason)
        {
            if (string.IsNullOrWhiteSpace(returnReason))
                throw new ArgumentException("دلیل مرجوعی نباید خالی باشد");
            ReturnReason = returnReason.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("تعداد باید بزرگتر از صفر باشد");
            Quantity = quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRefundAmount(decimal refundAmount)
        {
            if (refundAmount < 0)
                throw new ArgumentException("مبلغ بازگشت نمی‌تواند منفی باشد");
            RefundAmount = refundAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAdminNotes(string? adminNotes)
        {
            AdminNotes = adminNotes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Approve(string? approvedBy, string? adminNotes)
        {
            ReturnStatus = "Approved";
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            SetAdminNotes(adminNotes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string? rejectedBy, string? rejectionReason, string? adminNotes)
        {
            ReturnStatus = "Rejected";
            RejectedAt = DateTime.UtcNow;
            RejectedBy = rejectedBy;
            RejectionReason = rejectionReason?.Trim();
            SetAdminNotes(adminNotes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Process(string? processedBy)
        {
            ReturnStatus = "Processed";
            ProcessedAt = DateTime.UtcNow;
            ProcessedBy = processedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string returnReason, string? description, int quantity, 
            decimal refundAmount, string? updatedBy)
        {
            SetReturnReason(returnReason);
            SetDescription(description);
            SetQuantity(quantity);
            SetRefundAmount(refundAmount);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این درخواست مرجوعی قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
