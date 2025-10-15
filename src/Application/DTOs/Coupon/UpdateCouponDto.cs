using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Application.DTOs.Coupon
{
    public class UpdateCouponDto
    {
        [Required(ErrorMessage = "شناسه کوپن الزامی است")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "نام کوپن الزامی است")]
        [StringLength(100, ErrorMessage = "نام کوپن نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "درصد تخفیف باید بین 0 تا 100 باشد")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "مبلغ تخفیف نمی‌تواند منفی باشد")]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حداقل خرید نمی‌تواند منفی باشد")]
        public decimal MinimumPurchase { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "حداکثر تخفیف نمی‌تواند منفی باشد")]
        public decimal MaximumDiscount { get; set; }

        [Required(ErrorMessage = "تاریخ شروع الزامی است")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاریخ پایان الزامی است")]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "حد استفاده نمی‌تواند منفی باشد")]
        public int UsageLimit { get; set; }

        public bool IsSingleUse { get; set; } = false;

        [StringLength(1000, ErrorMessage = "دسته‌بندی‌های قابل اعمال نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string? ApplicableCategories { get; set; }

        [StringLength(1000, ErrorMessage = "محصولات قابل اعمال نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string? ApplicableProducts { get; set; }

        [StringLength(1000, ErrorMessage = "کاربران قابل اعمال نمی‌تواند بیش از 1000 کاراکتر باشد")]
        public string? ApplicableUsers { get; set; }
    }
}
