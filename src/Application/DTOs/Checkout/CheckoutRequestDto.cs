namespace OnlineShop.Application.DTOs.Checkout
{
    public class CheckoutRequestDto
    {
        public Guid CartId { get; set; }
        public Guid ShippingAddressId { get; set; }
        public Guid? BillingAddressId { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? CouponCode { get; set; }
        public string? Notes { get; set; }
    }
}
