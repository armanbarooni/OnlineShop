namespace OnlineShop.Application.DTOs.StockAlert
{
    public class StockAlertDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool Notified { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public string? NotificationMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation Properties
        public string? ProductName { get; set; }
        public string? ProductVariantName { get; set; }
        public string? UserName { get; set; }
    }
}
