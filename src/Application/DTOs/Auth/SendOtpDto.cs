namespace OnlineShop.Application.DTOs.Auth
{
    public class SendOtpDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Purpose { get; set; } = "Login"; // "Login", "Registration", "PasswordReset"
    }
}

