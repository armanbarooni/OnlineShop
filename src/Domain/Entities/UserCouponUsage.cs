using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// User coupon usage tracking entity
    /// Tracks when and how users use coupons
    /// </summary>
    public class UserCouponUsage : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public Guid CouponId { get; private set; }
        public Guid? OrderId { get; private set; }
        public DateTime UsedAt { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal OrderTotal { get; private set; }
        public string? Notes { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual Coupon Coupon { get; private set; } = null!;
        public virtual UserOrder? Order { get; private set; }

        protected UserCouponUsage() { }

        private UserCouponUsage(
            string userId,
            Guid couponId,
            Guid? orderId,
            decimal discountAmount,
            decimal orderTotal,
            string? notes = null)
        {
            SetUserId(userId);
            SetCouponId(couponId);
            SetOrderId(orderId);
            SetDiscountAmount(discountAmount);
            SetOrderTotal(orderTotal);
            SetNotes(notes);
            UsedAt = DateTime.UtcNow;
            Deleted = false;
        }

        public static UserCouponUsage Create(
            string userId,
            Guid couponId,
            Guid? orderId,
            decimal discountAmount,
            decimal orderTotal,
            string? notes = null)
            => new(userId, couponId, orderId, discountAmount, orderTotal, notes);

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نباید خالی باشد");
            UserId = userId.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCouponId(Guid couponId)
        {
            if (couponId == Guid.Empty)
                throw new ArgumentException("شناسه کوپن نباید خالی باشد");
            CouponId = couponId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOrderId(Guid? orderId)
        {
            OrderId = orderId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountAmount(decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentException("مبلغ تخفیف نمی‌تواند منفی باشد");
            DiscountAmount = discountAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOrderTotal(decimal orderTotal)
        {
            if (orderTotal < 0)
                throw new ArgumentException("مجموع سفارش نمی‌تواند منفی باشد");
            OrderTotal = orderTotal;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateUsage(decimal discountAmount, decimal orderTotal, string? notes = null)
        {
            SetDiscountAmount(discountAmount);
            SetOrderTotal(orderTotal);
            SetNotes(notes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این رکورد استفاده از کوپن قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
