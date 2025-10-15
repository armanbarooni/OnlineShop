using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// Coupon entity for discount management
    /// Supports percentage and fixed amount discounts
    /// </summary>
    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal DiscountPercentage { get; private set; }
        public decimal DiscountAmount { get; private set; }
        public decimal MinimumPurchase { get; private set; }
        public decimal MaximumDiscount { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int UsageLimit { get; private set; }
        public int UsedCount { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsSingleUse { get; private set; } = false;
        public string? ApplicableCategories { get; private set; }
        public string? ApplicableProducts { get; private set; }
        public string? ApplicableUsers { get; private set; }

        // Navigation Properties
        public virtual ICollection<UserCouponUsage> UserCouponUsages { get; private set; } = new List<UserCouponUsage>();

        protected Coupon() { }

        private Coupon(
            string code,
            string name,
            string description,
            decimal discountPercentage,
            decimal discountAmount,
            decimal minimumPurchase,
            decimal maximumDiscount,
            DateTime startDate,
            DateTime endDate,
            int usageLimit,
            bool isSingleUse,
            string? applicableCategories = null,
            string? applicableProducts = null,
            string? applicableUsers = null)
        {
            SetCode(code);
            SetName(name);
            SetDescription(description);
            SetDiscountPercentage(discountPercentage);
            SetDiscountAmount(discountAmount);
            SetMinimumPurchase(minimumPurchase);
            SetMaximumDiscount(maximumDiscount);
            SetStartDate(startDate);
            SetEndDate(endDate);
            SetUsageLimit(usageLimit);
            SetIsSingleUse(isSingleUse);
            SetApplicableCategories(applicableCategories);
            SetApplicableProducts(applicableProducts);
            SetApplicableUsers(applicableUsers);
            IsActive = true;
            UsedCount = 0;
            Deleted = false;
        }

        public static Coupon Create(
            string code,
            string name,
            string description,
            decimal discountPercentage,
            decimal discountAmount,
            decimal minimumPurchase,
            decimal maximumDiscount,
            DateTime startDate,
            DateTime endDate,
            int usageLimit,
            bool isSingleUse = false,
            string? applicableCategories = null,
            string? applicableProducts = null,
            string? applicableUsers = null)
            => new(code, name, description, discountPercentage, discountAmount, minimumPurchase, maximumDiscount, startDate, endDate, usageLimit, isSingleUse, applicableCategories, applicableProducts, applicableUsers);

        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد کوپن نباید خالی باشد");
            Code = code.Trim().ToUpper();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام کوپن نباید خالی باشد");
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountPercentage(decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("درصد تخفیف باید بین 0 تا 100 باشد");
            DiscountPercentage = discountPercentage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountAmount(decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentException("مبلغ تخفیف نمی‌تواند منفی باشد");
            DiscountAmount = discountAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMinimumPurchase(decimal minimumPurchase)
        {
            if (minimumPurchase < 0)
                throw new ArgumentException("حداقل خرید نمی‌تواند منفی باشد");
            MinimumPurchase = minimumPurchase;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMaximumDiscount(decimal maximumDiscount)
        {
            if (maximumDiscount < 0)
                throw new ArgumentException("حداکثر تخفیف نمی‌تواند منفی باشد");
            MaximumDiscount = maximumDiscount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStartDate(DateTime startDate)
        {
            StartDate = startDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEndDate(DateTime endDate)
        {
            if (endDate <= StartDate)
                throw new ArgumentException("تاریخ پایان باید بعد از تاریخ شروع باشد");
            EndDate = endDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetUsageLimit(int usageLimit)
        {
            if (usageLimit < 0)
                throw new ArgumentException("حد استفاده نمی‌تواند منفی باشد");
            UsageLimit = usageLimit;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetIsSingleUse(bool isSingleUse)
        {
            IsSingleUse = isSingleUse;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetApplicableCategories(string? applicableCategories)
        {
            ApplicableCategories = applicableCategories?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetApplicableProducts(string? applicableProducts)
        {
            ApplicableProducts = applicableProducts?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetApplicableUsers(string? applicableUsers)
        {
            ApplicableUsers = applicableUsers?.Trim();
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

        public void IncrementUsage()
        {
            if (UsedCount >= UsageLimit && UsageLimit > 0)
                throw new InvalidOperationException("حد استفاده از کوپن تمام شده است");
            
            UsedCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            return IsActive && 
                   !Deleted && 
                   now >= StartDate && 
                   now <= EndDate && 
                   (UsageLimit == 0 || UsedCount < UsageLimit);
        }

        public decimal CalculateDiscount(decimal orderTotal)
        {
            if (!IsValid() || orderTotal < MinimumPurchase)
                return 0;

            decimal discount = 0;

            if (DiscountPercentage > 0)
            {
                discount = orderTotal * (DiscountPercentage / 100);
                if (MaximumDiscount > 0 && discount > MaximumDiscount)
                    discount = MaximumDiscount;
            }
            else if (DiscountAmount > 0)
            {
                discount = DiscountAmount;
            }

            return Math.Min(discount, orderTotal);
        }

        public void Update(
            string name,
            string description,
            decimal discountPercentage,
            decimal discountAmount,
            decimal minimumPurchase,
            decimal maximumDiscount,
            DateTime startDate,
            DateTime endDate,
            int usageLimit,
            bool isSingleUse,
            string? applicableCategories = null,
            string? applicableProducts = null,
            string? applicableUsers = null,
            string? updatedBy = null)
        {
            SetName(name);
            SetDescription(description);
            SetDiscountPercentage(discountPercentage);
            SetDiscountAmount(discountAmount);
            SetMinimumPurchase(minimumPurchase);
            SetMaximumDiscount(maximumDiscount);
            SetStartDate(startDate);
            SetEndDate(endDate);
            SetUsageLimit(usageLimit);
            SetIsSingleUse(isSingleUse);
            SetApplicableCategories(applicableCategories);
            SetApplicableProducts(applicableProducts);
            SetApplicableUsers(applicableUsers);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این کوپن قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
