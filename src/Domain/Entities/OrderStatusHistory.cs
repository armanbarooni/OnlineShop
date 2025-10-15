using OnlineShop.Domain.Common;
using OnlineShop.Domain.Enums;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Order status history entity
    /// Tracks all status changes for an order with timestamps and notes
    /// </summary>
    public class OrderStatusHistory : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public string? Note { get; private set; }
        public DateTime ChangedAt { get; private set; }
        public string? ChangedBy { get; private set; } // User ID or system

        // Navigation Properties
        public virtual UserOrder Order { get; private set; } = null!;

        protected OrderStatusHistory() { }

        private OrderStatusHistory(
            Guid orderId,
            OrderStatus status,
            string? note = null,
            string? changedBy = null)
        {
            SetOrderId(orderId);
            SetStatus(status);
            SetNote(note);
            SetChangedBy(changedBy);
            ChangedAt = DateTime.UtcNow;
            Deleted = false;
        }

        public static OrderStatusHistory Create(
            Guid orderId,
            OrderStatus status,
            string? note = null,
            string? changedBy = null)
            => new(orderId, status, note, changedBy);

        public void SetOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("شناسه سفارش نباید خالی باشد");
            OrderId = orderId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStatus(OrderStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNote(string? note)
        {
            Note = note?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetChangedBy(string? changedBy)
        {
            ChangedBy = changedBy?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus, string? note = null, string? changedBy = null)
        {
            SetStatus(newStatus);
            SetNote(note);
            SetChangedBy(changedBy);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این رکورد تاریخچه سفارش قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
