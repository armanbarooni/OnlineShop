using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class MahakSyncLogRepository : IMahakSyncLogRepository
    {
        private readonly ApplicationDbContext _context;

        public MahakSyncLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MahakSyncLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(msl => msl.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MahakSyncLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .Where(msl => msl.EntityType == entityType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakSyncLog>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .Where(msl => msl.EntityId == entityId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakSyncLog>> GetBySyncStatusAsync(string syncStatus, CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .Where(msl => msl.SyncStatus == syncStatus)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakSyncLog>> GetFailedSyncsAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .Where(msl => msl.SyncStatus == "Failed")
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakSyncLog>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakSyncLogs
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(MahakSyncLog mahakSyncLog, CancellationToken cancellationToken)
        {
            await _context.MahakSyncLogs.AddAsync(mahakSyncLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MahakSyncLog mahakSyncLog, CancellationToken cancellationToken)
        {
            _context.MahakSyncLogs.Update(mahakSyncLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var mahakSyncLog = await _context.MahakSyncLogs.FindAsync(id, cancellationToken);
            if (mahakSyncLog != null)
            {
                _context.MahakSyncLogs.Remove(mahakSyncLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanupOldLogsAsync(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var oldLogs = await _context.MahakSyncLogs
                .Where(msl => msl.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldLogs.Any())
            {
                _context.MahakSyncLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
