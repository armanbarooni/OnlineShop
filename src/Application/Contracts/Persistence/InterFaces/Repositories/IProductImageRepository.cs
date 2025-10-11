using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IProductImageRepository
    {
        Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<ProductImage?> GetPrimaryImageAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductImage>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(ProductImage productImage, CancellationToken cancellationToken);
        Task UpdateAsync(ProductImage productImage, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task SetPrimaryImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
    }
}
