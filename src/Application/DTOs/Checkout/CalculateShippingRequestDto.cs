namespace OnlineShop.Application.DTOs.Checkout
{
    public class CalculateShippingRequestDto
    {
        public string ShippingMethodId { get; set; } = string.Empty;
        public Guid AddressId { get; set; }
        public decimal? OrderWeight { get; set; }
        public decimal? OrderValue { get; set; }
    }
}

