using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string SessionId { get; private set; } = string.Empty;
        public string CartName { get; private set; } = "Default";
        public bool IsActive { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string? Notes { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; private set; } = new List<CartItem>();

        protected Cart() { }

        private Cart(Guid userId, string sessionId, string cartName, bool isActive, DateTime? expiresAt)
        {
            UserId = userId;
            SetSessionId(sessionId);
            SetCartName(cartName);
            SetIsActive(isActive);
            ExpiresAt = expiresAt; // Set directly in constructor without validation
            Deleted = false;
        }

        public static Cart Create(Guid userId, string sessionId, string cartName = "Default", 
            bool isActive = true, DateTime? expiresAt = null)
            => new(userId, sessionId, cartName, isActive, expiresAt);

        public void SetSessionId(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("شناسه جلسه نباید خالی باشد");
            SessionId = sessionId.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCartName(string cartName)
        {
            if (string.IsNullOrWhiteSpace(cartName))
                throw new ArgumentException("نام سبد خرید نباید خالی باشد");
            CartName = cartName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetExpiresAt(DateTime? expiresAt)
        {
            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("تاریخ انقضا باید در آینده باشد");
            ExpiresAt = expiresAt;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ExtendExpiration(DateTime newExpirationDate)
        {
            SetExpiresAt(newExpirationDate);
        }

        public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

        public void Update(string cartName, bool isActive, DateTime? expiresAt, string? notes, string? updatedBy)
        {
            SetCartName(cartName);
            SetIsActive(isActive);
            SetExpiresAt(expiresAt);
            SetNotes(notes);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این سبد خرید قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
