namespace OnlineShop.Application.DTOs.UserProductView
{
    public class UserProductViewDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public DateTime ViewedAt { get; set; }
        public string? SessionId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
