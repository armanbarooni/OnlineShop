using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.StockAlert;

namespace OnlineShop.Application.Features.StockAlert.Commands.CreateStockAlert
{
    public class CreateStockAlertCommand : IRequest<Result<StockAlertResultDto>>
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? NotificationMethod { get; set; }
    }
}
