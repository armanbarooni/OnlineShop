namespace OnlineShop.Application.DTOs.Auth
{
    public class LoginWithPhoneDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}

