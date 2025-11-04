namespace OnlineShop.Application.DTOs.Checkout
{
    public class ShippingMethodDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int EstimatedDays { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}

