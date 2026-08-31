using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Services
{
    public interface IInventoryService
    {
        Task<bool> CheckStockAvailability(Guid productId, int quantity, CancellationToken cancellationToken);
        Task ReserveStockForOrder(Guid orderId, List<(Guid ProductId, Guid? VariantId, int Quantity)> items, CancellationToken cancellationToken);
        Task CommitOrder(Guid orderId, CancellationToken cancellationToken);
        Task ReleaseStockForCancelledOrder(Guid orderId, CancellationToken cancellationToken);
        Task<int> GetAvailableStock(Guid productId, CancellationToken cancellationToken);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;

        public InventoryService(
            IProductInventoryRepository inventoryRepository,
            IUserOrderItemRepository orderItemRepository,
            IProductRepository productRepository)
        {
            _inventoryRepository = inventoryRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> CheckStockAvailability(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
            if (inventory == null)
                return false;

            return inventory.GetAvailableStock() >= quantity;
        }

        public async Task ReserveStockForOrder(Guid orderId, List<(Guid ProductId, Guid? VariantId, int Quantity)> items, CancellationToken cancellationToken)
        {
            const int maxRetries = 3;
            var attempt = 0;
            while (true)
            {
                attempt++;
                try
                {
                    var reserved = await _inventoryRepository.TryReserveMultipleAsync(orderId, items, cancellationToken);
                    if (!reserved)
                    {
                        // Insufficient stock for at least one item
                        throw new InvalidOperationException("موجودی کافی برای تکمیل سفارش وجود ندارد");
                    }

                    break; // success
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    // optimistic concurrency conflict - retry entire order reservation
                    await Task.Delay(50, cancellationToken);
                    continue;
                }
            }
        }

        public async Task CommitOrder(Guid orderId, CancellationToken cancellationToken)
        {
            var orderItems = (await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken)).ToList();
            var soldAt = TruncateToSecond(DateTime.UtcNow);
            await _inventoryRepository.CommitReservationsAsync(
                orderItems.Select(i => (i.ProductId, i.VariantId, i.Quantity)), soldAt, cancellationToken);
            await ReduceCatalogStockAsync(orderItems, soldAt, cancellationToken);
        }

        private async Task ReduceCatalogStockAsync(
            List<UserOrderItem> orderItems,
            DateTime soldAt,
            CancellationToken cancellationToken)
        {
            foreach (var productGroup in orderItems.GroupBy(i => i.ProductId))
            {
                var product = await _productRepository.GetByIdAsync(productGroup.Key, cancellationToken);
                if (product == null)
                {
                    continue;
                }

                product.ReduceStock(productGroup.Sum(i => i.Quantity), soldAt);
                await _productRepository.UpdateAsync(product, cancellationToken);
            }
        }

        private static DateTime TruncateToSecond(DateTime value)
        {
            return new DateTime(value.Ticks - value.Ticks % TimeSpan.TicksPerSecond, value.Kind);
        }

        public async Task ReleaseStockForCancelledOrder(Guid orderId, CancellationToken cancellationToken)
        {
            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
            await _inventoryRepository.ReleaseReservationsAsync(
                orderItems.Select(i => (i.ProductId, i.VariantId, i.Quantity)), cancellationToken);
        }

        public async Task<int> GetAvailableStock(Guid productId, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
            return inventory?.GetAvailableStock() ?? 0;
        }
    }
}

