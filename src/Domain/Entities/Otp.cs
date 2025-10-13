using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// One-Time Password entity for phone number verification
    /// </summary>
    public class Otp : BaseEntity
    {
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public string? UsedFor { get; private set; } // "Registration", "Login", "PasswordReset"
        public int AttemptsCount { get; private set; }

        protected Otp() { }

        private Otp(string phoneNumber, string code, int expirationMinutes, string? usedFor = null)
        {
            SetPhoneNumber(phoneNumber);
            SetCode(code);
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
            IsUsed = false;
            AttemptsCount = 0;
            UsedFor = usedFor;
            Deleted = false;
        }

        public static Otp Create(string phoneNumber, string code, int expirationMinutes, string? usedFor = null)
            => new(phoneNumber, code, expirationMinutes, usedFor);

        public void SetPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نباید خالی باشد");
            
            PhoneNumber = phoneNumber.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد OTP نباید خالی باشد");
            
            Code = code.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        public bool IsValid()
        {
            return !IsUsed && !IsExpired() && !Deleted;
        }

        public void MarkAsUsed()
        {
            if (IsUsed)
                throw new InvalidOperationException("این کد OTP قبلاً استفاده شده است");
            
            if (IsExpired())
                throw new InvalidOperationException("این کد OTP منقضی شده است");
            
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementAttempts()
        {
            AttemptsCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasExceededMaxAttempts(int maxAttempts = 5)
        {
            return AttemptsCount >= maxAttempts;
        }

        public void Delete(string? deletedBy = null)
        {
            Deleted = true;
            UpdatedBy = deletedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

