namespace OnlineShop.Application.DTOs.Checkout
{
    public class CheckoutValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}

