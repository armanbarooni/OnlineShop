namespace OnlineShop.Application.DTOs.Checkout
{
    public class RemoveCouponFromCheckoutResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal FinalAmount { get; set; }
    }
}

