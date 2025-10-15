using OnlineShop.Application.DTOs.StockAlert;

namespace OnlineShop.Application.Contracts.Services
{
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
        Task<bool> SendStockAlertNotificationAsync(StockAlertDto stockAlert, string productName, CancellationToken cancellationToken = default);
        Task<bool> SendOrderStatusUpdateAsync(string userId, string orderNumber, string status, string? trackingNumber = null, CancellationToken cancellationToken = default);
        Task<bool> SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
    }
}
