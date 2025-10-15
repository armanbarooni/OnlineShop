using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductRelationRepository
    {
        Task<ProductRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ProductRelation>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<ProductRelation>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<List<ProductRelation>> GetRelatedProductsAsync(Guid productId, string relationType, int limit = 10, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid productId, Guid relatedProductId, CancellationToken cancellationToken = default);
        Task AddAsync(ProductRelation productRelation, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductRelation productRelation, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
