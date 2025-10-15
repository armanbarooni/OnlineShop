namespace OnlineShop.Application.DTOs.Coupon
{
    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public CouponDto? Coupon { get; set; }
    }
}
