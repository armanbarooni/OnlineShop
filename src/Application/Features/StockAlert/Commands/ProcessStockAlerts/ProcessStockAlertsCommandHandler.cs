using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Interfaces.Repositories;
using System.Linq;

namespace OnlineShop.Application.Features.StockAlert.Commands.ProcessStockAlerts
{
    public class ProcessStockAlertsCommandHandler : IRequestHandler<ProcessStockAlertsCommand, Result<int>>
    {
        private readonly IStockAlertRepository _stockAlertRepository;
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;

        public ProcessStockAlertsCommandHandler(
            IStockAlertRepository stockAlertRepository,
            IProductInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            INotificationService notificationService)
        {
            _stockAlertRepository = stockAlertRepository;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _notificationService = notificationService;
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
                        // Get product name for notification
                        var product = await _productRepository.GetByIdAsync(alert.ProductId, cancellationToken);
                        if (product != null)
                        {
                            // Send notification
                            var stockAlertDto = new OnlineShop.Application.DTOs.StockAlert.StockAlertDto
                            {
                                Id = alert.Id,
                                ProductId = alert.ProductId,
                                ProductVariantId = alert.ProductVariantId,
                                UserId = alert.UserId,
                                Email = alert.Email,
                                PhoneNumber = alert.PhoneNumber,
                                NotificationMethod = alert.NotificationMethod
                            };

                            var notificationSent = await _notificationService.SendStockAlertNotificationAsync(
                                stockAlertDto, 
                                product.Name, 
                                cancellationToken);

                            if (notificationSent)
                            {
                                // Mark alert as notified
                                alert.MarkAsNotified("System");
                                await _stockAlertRepository.UpdateAsync(alert, cancellationToken);
                                processedCount++;
                            }
                        }
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
