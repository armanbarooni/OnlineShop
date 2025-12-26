namespace OnlineShop.Application.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
