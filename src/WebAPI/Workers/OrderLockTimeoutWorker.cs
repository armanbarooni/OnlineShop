using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Interfaces.Services;
using OnlineShop.Domain.Entities;

namespace OnlineShop.WebAPI.Workers
{
    public class OrderLockTimeoutWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderLockTimeoutWorker> _logger;

        public OrderLockTimeoutWorker(
            IServiceProvider serviceProvider,
            ILogger<OrderLockTimeoutWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderLockTimeoutWorker starting - will check for expired orders every minute");

            // Check every 1 minute
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var orderRepository = scope.ServiceProvider.GetRequiredService<IUserOrderRepository>();
                    var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                    // Get orders pending for more than 10 minutes
                    var expiredOrders = await orderRepository.GetExpiredPendingOrdersAsync(10, stoppingToken);

                    if (expiredOrders != null && expiredOrders.Any())
                    {
                        _logger.LogInformation("Found {Count} expired pending orders. Cancelling them...", expiredOrders.Count);

                        foreach (var order in expiredOrders)
                        {
                            try
                            {
                                _logger.LogInformation("Cancelling order {OrderId} due to payment timeout", order.Id);

                                // Release first. If this fails, the order remains Pending and is
                                // retried on the next worker pass instead of leaving a permanent lock.
                                await inventoryService.ReleaseStockForCancelledOrder(order.Id, stoppingToken);

                                order.Cancel("عدم پرداخت در مهلت مقرر (10 دقیقه)");
                                await orderRepository.UpdateAsync(order, stoppingToken);

                                _logger.LogInformation("Successfully cancelled order {OrderId} and released stock locks", order.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to cancel order {OrderId} in timeout worker", order.Id);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OrderLockTimeoutWorker");
                }

                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
        }
    }
}
