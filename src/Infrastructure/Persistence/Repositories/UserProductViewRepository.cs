using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserProductViewRepository : IUserProductViewRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProductViewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<UserProductView?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.User)
                .Include(upv => upv.Product)
                .FirstOrDefaultAsync(upv => upv.Id == id && !upv.Deleted, cancellationToken);

        public Task<List<UserProductView>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.UserId == userId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetRecentlyViewedAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.UserId == userId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetReturningViewsAsync(Guid userId, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.UserId == userId && upv.IsReturningView && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .ToListAsync(cancellationToken);

        public async Task<bool> HasUserViewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.UserProductViews
                .AnyAsync(upv => upv.UserId == userId && upv.ProductId == productId && !upv.Deleted, cancellationToken);
        }

        public Task<List<UserProductView>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.User)
                .Where(upv => upv.ProductId == productId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .ToListAsync(cancellationToken);

        public async Task<List<Guid>> GetFrequentlyBoughtTogetherAsync(Guid productId, int limit = 10, CancellationToken cancellationToken = default)
        {
            // Get users who bought this product
            var usersWhoBoughtProduct = await _context.UserOrderItems
                .Where(oi => oi.ProductId == productId)
                .Select(oi => oi.Order.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!usersWhoBoughtProduct.Any())
                return new List<Guid>();

            // Get other products bought by the same users
            var frequentlyBoughtTogether = await _context.UserOrderItems
                .Where(oi => usersWhoBoughtProduct.Contains(oi.Order.UserId) && oi.ProductId != productId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .Select(x => x.ProductId)
                .ToListAsync(cancellationToken);

            return frequentlyBoughtTogether;
        }

        public async Task AddAsync(UserProductView userProductView, CancellationToken cancellationToken = default)
        {
            await _context.UserProductViews.AddAsync(userProductView, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserProductView userProductView, CancellationToken cancellationToken = default)
        {
            _context.UserProductViews.Update(userProductView);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userProductView = await _context.UserProductViews.FindAsync(new object[] { id }, cancellationToken);
            if (userProductView != null)
            {
                userProductView.Delete(null);
                _context.UserProductViews.Update(userProductView);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public Task<List<UserProductView>> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.SessionId == sessionId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetByDeviceTypeAsync(string deviceType, int limit = 50, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.DeviceType == deviceType && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetByBrowserAsync(string browser, int limit = 50, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.Browser == browser && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetLongDurationViewsAsync(int minDurationSeconds = 60, int limit = 50, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.ViewDuration >= minDurationSeconds && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewDuration)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public async Task<Dictionary<string, int>> GetDeviceTypeStatsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserProductViews
                .Where(upv => !upv.Deleted && !string.IsNullOrEmpty(upv.DeviceType))
                .GroupBy(upv => upv.DeviceType)
                .Select(g => new { DeviceType = g.Key!, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeviceType, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetBrowserStatsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserProductViews
                .Where(upv => !upv.Deleted && !string.IsNullOrEmpty(upv.Browser))
                .GroupBy(upv => upv.Browser)
                .Select(g => new { Browser = g.Key!, Count = g.Count() })
                .ToDictionaryAsync(x => x.Browser, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetOperatingSystemStatsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserProductViews
                .Where(upv => !upv.Deleted && !string.IsNullOrEmpty(upv.OperatingSystem))
                .GroupBy(upv => upv.OperatingSystem)
                .Select(g => new { OperatingSystem = g.Key!, Count = g.Count() })
                .ToDictionaryAsync(x => x.OperatingSystem, x => x.Count, cancellationToken);
        }

        public async Task DeleteOldViewsAsync(int daysOld = 90, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldViews = await _context.UserProductViews
                .Where(upv => upv.ViewedAt < cutoffDate && !upv.Deleted)
                .ToListAsync(cancellationToken);

            foreach (var view in oldViews)
            {
                view.Delete("System");
            }

            if (oldViews.Any())
            {
                _context.UserProductViews.UpdateRange(oldViews);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
