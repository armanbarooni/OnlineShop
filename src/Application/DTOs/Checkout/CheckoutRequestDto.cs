namespace OnlineShop.Application.DTOs.Checkout
{
    public class CheckoutRequestDto
    {
        public Guid CartId { get; set; }
        public Guid ShippingAddressId { get; set; }
        public Guid? BillingAddressId { get; set; } // Optional, defaults to shipping address
        public decimal ShippingCost { get; set; }
        public decimal TaxRate { get; set; } = 0.09m; // 9% default tax
        public decimal DiscountAmount { get; set; } = 0;
        public string? Notes { get; set; }
    }
}

