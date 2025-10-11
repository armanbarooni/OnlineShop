using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface ISavedCartRepository
    {
        Task<SavedCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<SavedCart>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<SavedCart>> GetFavoriteCartsAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<SavedCart>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(SavedCart savedCart, CancellationToken cancellationToken);
        Task UpdateAsync(SavedCart savedCart, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task RecordAccessAsync(Guid id, CancellationToken cancellationToken);
    }
}
