using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

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
            foreach (var (productId, quantity) in items)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(productId, cancellationToken);
                if (inventory == null)
                    throw new InvalidOperationException($"موجودی محصول {productId} یافت نشد");

                inventory.ReserveQuantity(quantity);
                await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
            }
        }

        public async Task ReleaseStockForCancelledOrder(Guid orderId, CancellationToken cancellationToken)
        {
            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId, cancellationToken);
            
            foreach (var item in orderItems)
            {
                var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                if (inventory != null)
                {
                    try
                    {
                        inventory.ReleaseReservedQuantity(item.Quantity);
                        await _inventoryRepository.UpdateAsync(inventory, cancellationToken);
                    }
                    catch
                    {
                        // Log error but continue with other items
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

