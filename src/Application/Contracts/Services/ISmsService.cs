namespace OnlineShop.Application.Contracts.Services
{
    public interface ISmsService
    {
        /// <summary>
        /// Send SMS to a phone number
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number (e.g., 09123456789)</param>
        /// <param name="message">SMS message content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if SMS sent successfully, false otherwise</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send OTP code to a phone number using a template
        /// </summary>
        /// <param name="phoneNumber">Recipient phone number</param>
        /// <param name="code">OTP code</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if OTP sent successfully, false otherwise</returns>
        Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    }
}

