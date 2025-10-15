using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.StockAlert.Commands.ProcessStockAlerts
{
    public class ProcessStockAlertsCommand : IRequest<Result<int>>
    {
        public Guid? ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
    }
}
