namespace OnlineShop.Application.DTOs.Cart
{
    public class ApplyCouponToCartResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid CouponId { get; set; }
        public CartSummaryDto? CartSummary { get; set; }
    }

    public class CartSummaryDto
    {
        public int TotalItems { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "IRR";
    }
}

