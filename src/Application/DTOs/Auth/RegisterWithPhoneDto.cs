namespace OnlineShop.Application.DTOs.Auth
{
    public class RegisterWithPhoneDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; } // Optional: user can set password or login only with OTP
    }
}

