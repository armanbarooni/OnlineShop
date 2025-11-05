using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Services
{
    public interface IInventoryService
    {
        Task<bool> CheckStockAvailability(Guid productId, int quantity, CancellationToken cancellationToken);
        Task ReserveStockForOrder(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken);
        Task ReleaseStockForCancelledOrder(Guid orderId, CancellationToken cancellationToken);
        Task<int> GetAvailableStock(Guid productId, CancellationToken cancellationToken);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IProductInventoryRepository _inventoryRepository;
        private readonly IUserOrderItemRepository _orderItemRepository;

        public InventoryService(
            IProductInventoryRepository inventoryRepository,
            IUserOrderItemRepository orderItemRepository)
        {
            _inventoryRepository = inventoryRepository;
            _orderItemRepository = orderItemRepository;
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

