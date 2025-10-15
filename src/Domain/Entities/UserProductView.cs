using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// User product view tracking entity
    /// Tracks when users view products for recommendations
    /// </summary>
    public class UserProductView : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public Guid ProductId { get; private set; }
        public DateTime ViewedAt { get; private set; }
        public string? SessionId { get; private set; }
        public string? UserAgent { get; private set; }
        public string? IpAddress { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected UserProductView() { }

        private UserProductView(string userId, Guid productId, string? sessionId = null, string? userAgent = null, string? ipAddress = null)
        {
            SetUserId(userId);
            SetProductId(productId);
            ViewedAt = DateTime.UtcNow;
            SetSessionId(sessionId);
            SetUserAgent(userAgent);
            SetIpAddress(ipAddress);
            Deleted = false;
        }

        public static UserProductView Create(string userId, Guid productId, string? sessionId = null, string? userAgent = null, string? ipAddress = null)
            => new(userId, productId, sessionId, userAgent, ipAddress);

        public void SetUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نباید خالی باشد");
            UserId = userId.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProductId(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("شناسه محصول نباید خالی باشد");
            ProductId = productId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSessionId(string? sessionId)
        {
            SessionId = sessionId?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUserAgent(string? userAgent)
        {
            UserAgent = userAgent?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIpAddress(string? ipAddress)
        {
            IpAddress = ipAddress?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateViewTime()
        {
            ViewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این رکورد بازدید قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
