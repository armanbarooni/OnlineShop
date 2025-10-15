using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using System.Linq;

namespace OnlineShop.Application.Features.StockAlert.Commands.ProcessStockAlerts
{
    public class ProcessStockAlertsCommandHandler : IRequestHandler<ProcessStockAlertsCommand, Result<int>>
    {
        private readonly IStockAlertRepository _stockAlertRepository;
        private readonly IProductInventoryRepository _inventoryRepository;

        public ProcessStockAlertsCommandHandler(
            IStockAlertRepository stockAlertRepository,
            IProductInventoryRepository inventoryRepository)
        {
            _stockAlertRepository = stockAlertRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<Result<int>> Handle(ProcessStockAlertsCommand request, CancellationToken cancellationToken)
        {
            var processedCount = 0;

            try
            {
                // Get alerts for the specific product/variant
                var alerts = await _stockAlertRepository.GetAlertsForProductAsync(
                    request.ProductId ?? Guid.Empty, 
                    request.ProductVariantId, 
                    cancellationToken);

                foreach (var alert in alerts)
                {
                    // Check if product is now in stock
                    var inventory = await _inventoryRepository.GetByProductIdAsync(alert.ProductId, cancellationToken);
                    var isInStock = inventory != null && inventory.AvailableQuantity > 0;

                    if (isInStock)
                    {
                        // Mark alert as notified
                        alert.MarkAsNotified("System");
                        await _stockAlertRepository.UpdateAsync(alert, cancellationToken);
                        processedCount++;

                        // TODO: Send actual notification (email/SMS)
                        // This would be implemented with a notification service
                    }
                }

                return Result<int>.Success(processedCount);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"خطا در پردازش هشدارهای موجودی: {ex.Message}");
            }
        }
    }
}
