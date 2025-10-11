using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Cart?> GetCartBySessionIdAsync(string sessionId, CancellationToken cancellationToken);
        Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Cart>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Cart cart, CancellationToken cancellationToken);
        Task UpdateAsync(Cart cart, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeactivateExpiredCartsAsync(CancellationToken cancellationToken);
    }
}
