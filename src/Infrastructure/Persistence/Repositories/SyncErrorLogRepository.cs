using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class SyncErrorLogRepository : ISyncErrorLogRepository
    {
        private readonly ApplicationDbContext _context;

        public SyncErrorLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SyncErrorLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(sel => sel.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetByErrorTypeAsync(string errorType, CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .Where(sel => sel.ErrorType == errorType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .Where(sel => sel.EntityType == entityType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetByErrorSeverityAsync(string errorSeverity, CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .Where(sel => sel.ErrorSeverity == errorSeverity)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetUnresolvedErrorsAsync(CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .Where(sel => !sel.IsResolved)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetCriticalErrorsAsync(CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .Where(sel => sel.ErrorSeverity == "Critical")
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SyncErrorLog>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.SyncErrorLogs
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(SyncErrorLog syncErrorLog, CancellationToken cancellationToken)
        {
            await _context.SyncErrorLogs.AddAsync(syncErrorLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(SyncErrorLog syncErrorLog, CancellationToken cancellationToken)
        {
            _context.SyncErrorLogs.Update(syncErrorLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var syncErrorLog = await _context.SyncErrorLogs.FindAsync(id, cancellationToken);
            if (syncErrorLog != null)
            {
                _context.SyncErrorLogs.Remove(syncErrorLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task IncrementOccurrenceAsync(Guid id, CancellationToken cancellationToken)
        {
            var syncErrorLog = await _context.SyncErrorLogs.FindAsync(id, cancellationToken);
            if (syncErrorLog != null)
            {
                syncErrorLog.IncrementOccurrence();
                _context.SyncErrorLogs.Update(syncErrorLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanupResolvedErrorsAsync(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var resolvedErrors = await _context.SyncErrorLogs
                .Where(sel => sel.IsResolved && sel.ResolvedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (resolvedErrors.Any())
            {
                _context.SyncErrorLogs.RemoveRange(resolvedErrors);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
