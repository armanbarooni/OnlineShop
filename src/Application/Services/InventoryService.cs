using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Services
{
    public interface IInventoryService
    {
        Task<bool> CheckStockAvailability(Guid productId, int quantity, CancellationToken cancellationToken);
        Task ReserveStockForOrder(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken);
        Task CommitOrder(Guid orderId, CancellationToken cancellationToken);
        Task ReleaseStockForCancelledOrder(Guid orderId, CancellationToken cancellationToken);
        Task<int> GetAvailableStock(Guid productId, CancellationToken cancellationToken);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public InventoryService(
            IProductInventoryRepository inventoryRepository,
            IUserOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository)
        {
            _inventoryRepository = inventoryRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
        }

        public async Task<bool> CheckStockAvailability(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
            if (inventory == null)
                return false;

            return inventory.GetAvailableStock() >= quantity;
        }

        public async Task ReserveStockForOrder(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken)
        {
            const int maxRetries = 3;
            var attempt = 0;
            System.Console.Error.WriteLine($"[InventoryService] ReserveStockForOrder order={orderId} items={string.Join(", ", items.Select(i => $"{i.ProductId}:{i.Quantity}"))}");
            while (true)
            {
                attempt++;
                try
                {
                    var reserved = await _inventoryRepository.TryReserveMultipleAsync(items, cancellationToken);
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
            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
            var soldAt = TruncateToSecond(DateTime.UtcNow);
            const int maxRetries = 3;

            foreach (var item in orderItems)
            {
                var attempt = 0;
                while (true)
                {
                    attempt++;
                    var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                    if (inventory == null)
                    {
                        break;
                    }

                    try
                    {
                        if (inventory.ReservedQuantity >= item.Quantity)
                        {
                            inventory.CommitSale(item.Quantity, soldAt);
                            await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
                        }
                        else
                        {
                            // If reservation is missing, try to reduce from available directly?
                            // For safety in this project, assuming reservation exists.
                            // If not, we might want to just log usage.
                             throw new InvalidOperationException($"No reservation found for Product {item.ProductId} in Order {orderId}");
                        }
                        break; // success
                    }
                    catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                    {
                        await Task.Delay(50, cancellationToken);
                        continue;
                    }
                }
            }

            await ReduceCatalogStockAsync(orderItems.ToList(), soldAt, cancellationToken);
        }

        private async Task ReduceCatalogStockAsync(
            List<UserOrderItem> orderItems,
            DateTime soldAt,
            CancellationToken cancellationToken)
        {
            foreach (var variantGroup in orderItems
                .Where(i => i.VariantId.HasValue)
                .GroupBy(i => i.VariantId!.Value))
            {
                var variant = await _productVariantRepository.GetByIdAsync(variantGroup.Key, cancellationToken);
                if (variant == null)
                {
                    continue;
                }

                variant.ReduceStock(variantGroup.Sum(i => i.Quantity), soldAt);
                await _productVariantRepository.UpdateAsync(variant, cancellationToken);
            }

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

            const int maxRetries = 3;
            foreach (var item in orderItems)
            {
                var attempt = 0;
                while (true)
                {
                    attempt++;
                    var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                    if (inventory == null)
                        break;

                    try
                    {
                        inventory.ReleaseReservedQuantity(item.Quantity);
                        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
                        break; // success
                    }
                    catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                    {
                        // optimistic concurrency conflict - retry
                        await Task.Delay(50, cancellationToken);
                        continue;
                    }
                    catch
                    {
                        // Log error but continue with other items
                        break;
                    }
                }
            }
        }

        public async Task<int> GetAvailableStock(Guid productId, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
            return inventory?.GetAvailableStock() ?? 0;
        }
    }
}

