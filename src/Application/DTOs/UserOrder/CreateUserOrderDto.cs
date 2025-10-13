namespace OnlineShop.Application.DTOs.UserOrder
{
    public class CreateUserOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; } = 0;
        public decimal ShippingAmount { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "IRR";
        public string? Notes { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public Guid? BillingAddressId { get; set; }
    }
}
