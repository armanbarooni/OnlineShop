using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class MahakQueueRepository : IMahakQueueRepository
    {
        private readonly ApplicationDbContext _context;

        public MahakQueueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MahakQueue?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .FirstOrDefaultAsync(mq => mq.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetPendingItemsAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .Where(mq => mq.QueueStatus == "Pending")
                .OrderBy(mq => mq.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetRetryItemsAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .Where(mq => mq.QueueStatus == "Retry" && mq.RetryCount < mq.MaxRetries)
                .OrderBy(mq => mq.NextRetryAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetByQueueTypeAsync(string queueType, CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .Where(mq => mq.QueueType == queueType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .Where(mq => mq.EntityType == entityType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetFailedItemsAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .Where(mq => mq.QueueStatus == "Failed")
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakQueue>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakQueues
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(MahakQueue mahakQueue, CancellationToken cancellationToken)
        {
            await _context.MahakQueues.AddAsync(mahakQueue, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MahakQueue mahakQueue, CancellationToken cancellationToken)
        {
            _context.MahakQueues.Update(mahakQueue);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var mahakQueue = await _context.MahakQueues.FindAsync(id, cancellationToken);
            if (mahakQueue != null)
            {
                _context.MahakQueues.Remove(mahakQueue);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanupCompletedItemsAsync(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            var completedItems = await _context.MahakQueues
                .Where(mq => mq.QueueStatus == "Completed" && mq.ProcessedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (completedItems.Any())
            {
                _context.MahakQueues.RemoveRange(completedItems);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
