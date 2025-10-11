using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string? NationalCode { get; private set; }
        public DateTime? BirthDate { get; private set; }
        public string? Gender { get; private set; }
        public string? ProfileImageUrl { get; private set; }
        public string? Bio { get; private set; }
        public string? Website { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public bool IsPhoneVerified { get; private set; }
        public DateTime? EmailVerifiedAt { get; private set; }
        public DateTime? PhoneVerifiedAt { get; private set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; private set; } = null!;

        protected UserProfile() { }

        private UserProfile(Guid userId, string firstName, string lastName)
        {
            UserId = userId;
            SetFirstName(firstName);
            SetLastName(lastName);
            IsEmailVerified = false;
            IsPhoneVerified = false;
            Deleted = false;
        }

        public static UserProfile Create(Guid userId, string firstName, string lastName)
            => new(userId, firstName, lastName);

        public void SetFirstName(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نباید خالی باشد");
            FirstName = firstName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نباید خالی باشد");
            LastName = lastName.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNationalCode(string? nationalCode)
        {
            if (!string.IsNullOrWhiteSpace(nationalCode) && nationalCode.Length != 10)
                throw new ArgumentException("کد ملی باید 10 رقم باشد");
            NationalCode = nationalCode?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBirthDate(DateTime? birthDate)
        {
            if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
                throw new ArgumentException("تاریخ تولد نمی‌تواند در آینده باشد");
            BirthDate = birthDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetGender(string? gender)
        {
            if (!string.IsNullOrWhiteSpace(gender) && !new[] { "Male", "Female", "Other" }.Contains(gender))
                throw new ArgumentException("جنسیت باید Male، Female یا Other باشد");
            Gender = gender?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetProfileImageUrl(string? profileImageUrl)
        {
            ProfileImageUrl = profileImageUrl?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBio(string? bio)
        {
            Bio = bio?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetWebsite(string? website)
        {
            Website = website?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkEmailAsVerified()
        {
            IsEmailVerified = true;
            EmailVerifiedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkPhoneAsVerified()
        {
            IsPhoneVerified = true;
            PhoneVerifiedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string firstName, string lastName, string? nationalCode, DateTime? birthDate, 
            string? gender, string? profileImageUrl, string? bio, string? website, string? updatedBy)
        {
            SetFirstName(firstName);
            SetLastName(lastName);
            SetNationalCode(nationalCode);
            SetBirthDate(birthDate);
            SetGender(gender);
            SetProfileImageUrl(profileImageUrl);
            SetBio(bio);
            SetWebsite(website);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این پروفایل کاربر قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
