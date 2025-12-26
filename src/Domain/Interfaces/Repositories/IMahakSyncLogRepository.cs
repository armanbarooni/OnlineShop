using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IMahakSyncLogRepository
    {
        Task<MahakSyncLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<MahakSyncLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken);
        Task<IEnumerable<MahakSyncLog>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken);
        Task<IEnumerable<MahakSyncLog>> GetBySyncStatusAsync(string syncStatus, CancellationToken cancellationToken);
        Task<IEnumerable<MahakSyncLog>> GetFailedSyncsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<MahakSyncLog>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(MahakSyncLog mahakSyncLog, CancellationToken cancellationToken);
        Task UpdateAsync(MahakSyncLog mahakSyncLog, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task CleanupOldLogsAsync(int daysToKeep, CancellationToken cancellationToken);
        Task<long> GetLastRowVersionAsync(string entityType, CancellationToken cancellationToken);
    }
}
