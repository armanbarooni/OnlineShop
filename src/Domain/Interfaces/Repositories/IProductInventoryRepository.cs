using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductInventoryRepository
    {
        Task<ProductInventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<ProductInventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductInventory>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<ProductInventory>> GetLowStockItemsAsync(int threshold, CancellationToken cancellationToken);
        Task<IEnumerable<ProductInventory>> GetOutOfStockItemsAsync(CancellationToken cancellationToken);
        Task AddAsync(ProductInventory productInventory, CancellationToken cancellationToken);
        Task UpdateAsync(ProductInventory productInventory, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateStockAsync(Guid productId, int availableQuantity, int reservedQuantity, int soldQuantity, CancellationToken cancellationToken);
        // Attempt to reserve quantity for a product atomically. Returns true when reservation succeeded, false when insufficient stock.
        Task<bool> TryReserveAsync(Guid productId, int quantity, CancellationToken cancellationToken);
        // Attempt to reserve multiple items atomically. Returns true when all reservations succeeded; false if any item has insufficient stock.
        Task<bool> TryReserveMultipleAsync(IEnumerable<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken);
    }
}
