namespace OnlineShop.Application.DTOs.Checkout
{
    public class PaymentMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public string? Icon { get; set; }
    }
}

