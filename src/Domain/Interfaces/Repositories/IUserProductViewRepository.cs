using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserProductViewRepository
    {
        Task<UserProductView?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<UserProductView>> GetByUserIdAsync(string userId, int limit = 20, CancellationToken cancellationToken = default);
        Task<List<UserProductView>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<List<UserProductView>> GetRecentlyViewedAsync(string userId, int limit = 20, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetFrequentlyBoughtTogetherAsync(Guid productId, int limit = 10, CancellationToken cancellationToken = default);
        Task AddAsync(UserProductView userProductView, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserProductView userProductView, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteOldViewsAsync(int daysOld = 90, CancellationToken cancellationToken = default);
    }
}
