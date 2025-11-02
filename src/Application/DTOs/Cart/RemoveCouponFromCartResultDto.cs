namespace OnlineShop.Application.DTOs.Cart
{
    public class RemoveCouponFromCartResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CartSummaryDto? CartSummary { get; set; }
    }
}

