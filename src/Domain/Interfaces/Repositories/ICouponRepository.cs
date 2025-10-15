using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task UpdateAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get coupon by code
        /// </summary>
        Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if coupon code exists
        /// </summary>
        Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get active coupons
        /// </summary>
        Task<IEnumerable<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get expired coupons
        /// </summary>
        Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get coupons by user eligibility
        /// </summary>
        Task<IEnumerable<Coupon>> GetEligibleCouponsAsync(string userId, decimal orderTotal, CancellationToken cancellationToken = default);
    }
}
