using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.StockAlert;

namespace OnlineShop.Application.Features.StockAlert.Queries.GetStockAlerts
{
    public class GetStockAlertsQuery : IRequest<Result<List<StockAlertDto>>>
    {
        public string? UserId { get; set; }
        public Guid? ProductId { get; set; }
        public bool? Notified { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
