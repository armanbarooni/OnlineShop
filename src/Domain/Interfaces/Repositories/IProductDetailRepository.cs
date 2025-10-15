using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductDetailRepository
    {
        Task<ProductDetail?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductDetail>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductDetail>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(ProductDetail productDetail, CancellationToken cancellationToken);
        Task UpdateAsync(ProductDetail productDetail, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    }
}
