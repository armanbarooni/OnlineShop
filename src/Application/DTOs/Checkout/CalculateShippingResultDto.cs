namespace OnlineShop.Application.DTOs.Checkout
{
    public class CalculateShippingResultDto
    {
        public string ShippingMethodId { get; set; } = string.Empty;
        public string ShippingMethodName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public int EstimatedDays { get; set; }
        public string Currency { get; set; } = "IRR";
    }
}

