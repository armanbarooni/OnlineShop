using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == id && !c.Deleted, cancellationToken);
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Coupons
                .Where(c => !c.Deleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            await _context.Coupons.AddAsync(coupon, cancellationToken);
        }

        public Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            _context.Coupons.Update(coupon);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var coupon = await GetByIdAsync(id, cancellationToken);
            if (coupon != null)
            {
                coupon.Delete("System");
            }
        }

        public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && !c.Deleted, cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Coupons
                .AnyAsync(c => c.Code == code.ToUpper() && !c.Deleted, cancellationToken);
        }

        public async Task<IEnumerable<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Coupons
                .Where(c => !c.Deleted && c.IsActive && c.StartDate <= now && c.EndDate >= now)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Coupons
                .Where(c => !c.Deleted && c.EndDate < now)
                .OrderByDescending(c => c.EndDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Coupon>> GetEligibleCouponsAsync(string userId, decimal orderTotal, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.Coupons
                .Where(c => !c.Deleted && c.IsActive && c.StartDate <= now && c.EndDate >= now && c.MinimumPurchase <= orderTotal);

            // Filter by usage limit
            query = query.Where(c => c.UsageLimit == 0 || c.UsedCount < c.UsageLimit);

            // Filter by user eligibility if applicable
            query = query.Where(c => string.IsNullOrEmpty(c.ApplicableUsers) || c.ApplicableUsers.Contains(userId));

            return await query
                .OrderByDescending(c => c.DiscountPercentage)
                .ThenByDescending(c => c.DiscountAmount)
                .ToListAsync(cancellationToken);
        }
    }
}
