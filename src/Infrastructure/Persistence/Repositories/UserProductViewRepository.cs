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

        public Task<List<UserProductView>> GetByUserIdAsync(string userId, int limit = 20, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.UserId == userId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.User)
                .Where(upv => upv.ProductId == productId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .ToListAsync(cancellationToken);

        public Task<List<UserProductView>> GetRecentlyViewedAsync(string userId, int limit = 20, CancellationToken cancellationToken = default)
            => _context.UserProductViews
                .AsNoTracking()
                .Include(upv => upv.Product)
                .Where(upv => upv.UserId == userId && !upv.Deleted)
                .OrderByDescending(upv => upv.ViewedAt)
                .Take(limit)
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
