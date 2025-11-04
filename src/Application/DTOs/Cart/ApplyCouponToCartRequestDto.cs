namespace OnlineShop.Application.DTOs.Cart
{
    public class ApplyCouponToCartRequestDto
    {
        public string CouponCode { get; set; } = string.Empty;
        public Guid? CartId { get; set; }
    }
}

