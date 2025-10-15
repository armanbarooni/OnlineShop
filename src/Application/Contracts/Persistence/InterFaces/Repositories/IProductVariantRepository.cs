using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IProductVariantRepository
    {
        Task<ProductVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ProductVariant>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductVariant?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task AddAsync(ProductVariant variant, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductVariant variant, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

