namespace OnlineShop.Application.DTOs.Coupon
{
    public class ApplyCouponResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid CouponId { get; set; }
    }
}
