namespace OnlineShop.Application.Common.Models
{
    /// <summary>
    /// SMS service settings
    /// </summary>
    public class SmsSettings
    {
        public string Provider { get; set; } = "Mock"; // Mock, Kavenegar, Ghasedak
        public string ApiKey { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string OtpTemplate { get; set; } = "OnlineShopOTP"; // Template name in Kavenegar panel
        public int OtpExpirationMinutes { get; set; } = 5;
        public int OtpLength { get; set; } = 6;
    }
}

