using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Stock alert entity for notifying users when out-of-stock items become available
    /// </summary>
    public class StockAlert : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid? ProductVariantId { get; private set; }
        public Guid UserId { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string? PhoneNumber { get; private set; }
        public bool Notified { get; private set; } = false;
        public DateTime? NotifiedAt { get; private set; }
        public string? NotificationMethod { get; private set; } // Email, SMS, Both

        // Navigation Properties
        public virtual Product Product { get; private set; } = null!;
        public virtual ProductVariant? ProductVariant { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        protected StockAlert() { }

        private StockAlert(
            Guid productId,
            Guid? productVariantId,
            Guid userId,
            string email,
            string? phoneNumber = null,
            string? notificationMethod = null)
        {
            SetProductId(productId);
            SetProductVariantId(productVariantId);
            SetUserId(userId);
            SetEmail(email);
            SetPhoneNumber(phoneNumber);
            SetNotificationMethod(notificationMethod);
            Deleted = false;
        }

        public static StockAlert Create(
            Guid productId,
            Guid? productVariantId,
            Guid userId,
            string email,
            string? phoneNumber = null,
            string? notificationMethod = null)
            => new(productId, productVariantId, userId, email, phoneNumber, notificationMethod);

        public void SetProductId(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("شناسه محصول نباید خالی باشد");
            ProductId = productId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProductVariantId(Guid? productVariantId)
        {
            ProductVariantId = productVariantId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("شناسه کاربر نباید خالی باشد");
            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نباید خالی باشد");
            if (!IsValidEmail(email))
                throw new ArgumentException("فرمت ایمیل نامعتبر است");
            Email = email.Trim().ToLower();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPhoneNumber(string? phoneNumber)
        {
            PhoneNumber = phoneNumber?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNotificationMethod(string? notificationMethod)
        {
            NotificationMethod = notificationMethod?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsNotified(string notificationMethod)
        {
            if (Notified)
                throw new InvalidOperationException("این هشدار قبلاً ارسال شده است");

            Notified = true;
            NotifiedAt = DateTime.UtcNow;
            NotificationMethod = notificationMethod;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetNotification()
        {
            Notified = false;
            NotifiedAt = null;
            NotificationMethod = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsExpired(int daysToExpire = 90)
        {
            return CreatedAt.AddDays(daysToExpire) < DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این هشدار موجودی قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
