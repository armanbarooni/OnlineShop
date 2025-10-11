using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IMahakQueueRepository
    {
        Task<MahakQueue?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetPendingItemsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetRetryItemsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetByQueueTypeAsync(string queueType, CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetFailedItemsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<MahakQueue>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(MahakQueue mahakQueue, CancellationToken cancellationToken);
        Task UpdateAsync(MahakQueue mahakQueue, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task CleanupCompletedItemsAsync(int daysToKeep, CancellationToken cancellationToken);
    }
}
