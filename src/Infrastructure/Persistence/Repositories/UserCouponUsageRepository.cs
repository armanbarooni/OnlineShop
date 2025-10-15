using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserCouponUsageRepository : IUserCouponUsageRepository
    {
        private readonly ApplicationDbContext _context;

        public UserCouponUsageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserCouponUsage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .Include(ucu => ucu.Coupon)
                .Include(ucu => ucu.User)
                .Include(ucu => ucu.Order)
                .FirstOrDefaultAsync(ucu => ucu.Id == id && !ucu.Deleted, cancellationToken);
        }

        public async Task<IEnumerable<UserCouponUsage>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .Include(ucu => ucu.Coupon)
                .Include(ucu => ucu.User)
                .Include(ucu => ucu.Order)
                .Where(ucu => !ucu.Deleted)
                .OrderByDescending(ucu => ucu.UsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserCouponUsage userCouponUsage, CancellationToken cancellationToken = default)
        {
            await _context.UserCouponUsages.AddAsync(userCouponUsage, cancellationToken);
        }

        public async Task UpdateAsync(UserCouponUsage userCouponUsage, CancellationToken cancellationToken = default)
        {
            _context.UserCouponUsages.Update(userCouponUsage);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usage = await GetByIdAsync(id, cancellationToken);
            if (usage != null)
            {
                usage.Delete("System");
            }
        }

        public async Task<IEnumerable<UserCouponUsage>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .Include(ucu => ucu.Coupon)
                .Include(ucu => ucu.Order)
                .Where(ucu => ucu.UserId == userId && !ucu.Deleted)
                .OrderByDescending(ucu => ucu.UsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserCouponUsage>> GetByCouponIdAsync(Guid couponId, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .Include(ucu => ucu.User)
                .Include(ucu => ucu.Order)
                .Where(ucu => ucu.CouponId == couponId && !ucu.Deleted)
                .OrderByDescending(ucu => ucu.UsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserCouponUsage>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .Include(ucu => ucu.Coupon)
                .Include(ucu => ucu.User)
                .Where(ucu => ucu.OrderId == orderId && !ucu.Deleted)
                .OrderByDescending(ucu => ucu.UsedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasUserUsedCouponAsync(string userId, Guid couponId, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .AnyAsync(ucu => ucu.UserId == userId && ucu.CouponId == couponId && !ucu.Deleted, cancellationToken);
        }

        public async Task<int> GetUsageCountByUserAsync(string userId, Guid couponId, CancellationToken cancellationToken = default)
        {
            return await _context.UserCouponUsages
                .CountAsync(ucu => ucu.UserId == userId && ucu.CouponId == couponId && !ucu.Deleted, cancellationToken);
        }
    }
}
