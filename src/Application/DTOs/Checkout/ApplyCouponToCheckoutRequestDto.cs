namespace OnlineShop.Application.DTOs.Checkout
{
    public class ApplyCouponToCheckoutRequestDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public Guid CartId { get; set; }
    }
}

