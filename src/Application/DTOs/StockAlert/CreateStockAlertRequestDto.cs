namespace OnlineShop.Application.DTOs.StockAlert
{
    public class CreateStockAlertRequestDto
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? NotificationMethod { get; set; } // Email, SMS, Both
    }
}
