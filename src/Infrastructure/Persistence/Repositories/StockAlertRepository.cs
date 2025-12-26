using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Common;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class StockAlertRepository : GenericRepository<StockAlert>, IStockAlertRepository
    {
        public StockAlertRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<StockAlert>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .Where(sa => sa.ProductId == productId && !sa.Deleted)
                .OrderBy(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockAlert>> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .Where(sa => sa.ProductVariantId == productVariantId && !sa.Deleted)
                .OrderBy(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockAlert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .Where(sa => sa.UserId == userId && !sa.Deleted)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockAlert>> GetUnnotifiedAlertsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .Where(sa => !sa.Notified && !sa.Deleted)
                .OrderBy(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockAlert>> GetExpiredAlertsAsync(int daysToExpire = 90, CancellationToken cancellationToken = default)
        {
            var expiryDate = DateTime.UtcNow.AddDays(-daysToExpire);
            return await _context.StockAlerts
                .Where(sa => sa.CreatedAt < expiryDate && !sa.Deleted)
                .OrderBy(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid productId, Guid? productVariantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .AnyAsync(sa => sa.ProductId == productId && 
                               sa.ProductVariantId == productVariantId && 
                               sa.UserId == userId && 
                               !sa.Deleted, cancellationToken);
        }

        public async Task<List<StockAlert>> GetAlertsForProductAsync(Guid productId, Guid? productVariantId, CancellationToken cancellationToken = default)
        {
            return await _context.StockAlerts
                .Where(sa => sa.ProductId == productId && 
                           sa.ProductVariantId == productVariantId && 
                           !sa.Notified && 
                           !sa.Deleted)
                .OrderBy(sa => sa.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
