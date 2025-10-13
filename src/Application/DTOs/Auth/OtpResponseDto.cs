namespace OnlineShop.Application.DTOs.Auth
{
    public class OtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}

