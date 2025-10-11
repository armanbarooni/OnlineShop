using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
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
    }
}
