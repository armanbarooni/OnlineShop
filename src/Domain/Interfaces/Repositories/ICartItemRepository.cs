using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface ICartItemRepository
    {
        Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken);
        Task<CartItem?> GetByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<CartItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<CartItem>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(CartItem cartItem, CancellationToken cancellationToken);
        Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteByCartIdAsync(Guid cartId, CancellationToken cancellationToken);
        Task DeleteByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken);
    }
}
