namespace OnlineShop.Application.DTOs.Checkout
{
    public class ApplyCouponToCheckoutResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid CouponId { get; set; }
        public string? CouponName { get; set; }
        public string? CouponDescription { get; set; }
    }
}

