using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface ISyncErrorLogRepository
    {
        Task<SyncErrorLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetByErrorTypeAsync(string errorType, CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetByErrorSeverityAsync(string errorSeverity, CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetCriticalErrorsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<SyncErrorLog>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(SyncErrorLog syncErrorLog, CancellationToken cancellationToken);
        Task UpdateAsync(SyncErrorLog syncErrorLog, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task IncrementOccurrenceAsync(Guid id, CancellationToken cancellationToken);
        Task CleanupResolvedErrorsAsync(int daysToKeep, CancellationToken cancellationToken);
    }
}
