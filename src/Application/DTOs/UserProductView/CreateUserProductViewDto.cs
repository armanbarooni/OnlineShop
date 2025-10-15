namespace OnlineShop.Application.DTOs.UserProductView
{
    public class CreateUserProductViewDto
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string? SessionId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }
}
