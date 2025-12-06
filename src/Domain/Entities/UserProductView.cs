using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// User product view tracking entity
    /// Tracks when users view products for recommendations
    /// </summary>
    public class UserProductView : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid ProductId { get; private set; }
        public DateTime ViewedAt { get; private set; }
        public string? SessionId { get; private set; }
        public string? UserAgent { get; private set; }
        public string? IpAddress { get; private set; }
        public int ViewDuration { get; private set; } = 0; // Duration in seconds
        public string? ReferrerUrl { get; private set; }
        public string? DeviceType { get; private set; } // Mobile, Desktop, Tablet
        public string? Browser { get; private set; }
        public string? OperatingSystem { get; private set; }
        public bool IsReturningView { get; private set; } = false;

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected UserProductView() { }

        private UserProductView(Guid userId, Guid productId, string? sessionId = null, string? userAgent = null, string? ipAddress = null, 
            string? referrerUrl = null, string? deviceType = null, string? browser = null, string? operatingSystem = null)
        {
            SetUserId(userId);
            SetProductId(productId);
            ViewedAt = DateTime.UtcNow;
            SetSessionId(sessionId);
            SetUserAgent(userAgent);
            SetIpAddress(ipAddress);
            SetReferrerUrl(referrerUrl);
            SetDeviceType(deviceType);
            SetBrowser(browser);
            SetOperatingSystem(operatingSystem);
            Deleted = false;
        }

        public static UserProductView Create(Guid userId, Guid productId, string? sessionId = null, string? userAgent = null, string? ipAddress = null,
            string? referrerUrl = null, string? deviceType = null, string? browser = null, string? operatingSystem = null)
            => new(userId, productId, sessionId, userAgent, ipAddress, referrerUrl, deviceType, browser, operatingSystem);

        public void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("شناسه کاربر نباید خالی باشد");
            UserId = userId;
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

        public void SetReferrerUrl(string? referrerUrl)
        {
            ReferrerUrl = referrerUrl?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDeviceType(string? deviceType)
        {
            DeviceType = deviceType?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBrowser(string? browser)
        {
            Browser = browser?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOperatingSystem(string? operatingSystem)
        {
            OperatingSystem = operatingSystem?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetViewDuration(int durationInSeconds)
        {
            if (durationInSeconds < 0)
                throw new ArgumentException("مدت زمان بازدید نمی‌تواند منفی باشد");
            ViewDuration = durationInSeconds;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsReturningView()
        {
            IsReturningView = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateViewDuration(int additionalSeconds)
        {
            if (additionalSeconds < 0)
                throw new ArgumentException("مدت زمان اضافی نمی‌تواند منفی باشد");
            ViewDuration += additionalSeconds;
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
