using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IWishlistRepository
    {
        Task<Wishlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Wishlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Wishlist?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<Wishlist>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
        Task AddAsync(Wishlist wishlist, CancellationToken cancellationToken);
        Task UpdateAsync(Wishlist wishlist, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
    }
}
