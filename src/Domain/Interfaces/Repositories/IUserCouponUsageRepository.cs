using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserCouponUsageRepository
    {
        Task<UserCouponUsage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserCouponUsage>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(UserCouponUsage userCouponUsage, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserCouponUsage userCouponUsage, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get usage history for a user
        /// </summary>
        Task<IEnumerable<UserCouponUsage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get usage history for a coupon
        /// </summary>
        Task<IEnumerable<UserCouponUsage>> GetByCouponIdAsync(Guid couponId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get usage history for an order
        /// </summary>
        Task<IEnumerable<UserCouponUsage>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user has used a specific coupon
        /// </summary>
        Task<bool> HasUserUsedCouponAsync(Guid userId, Guid couponId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get usage count for a coupon by user
        /// </summary>
        Task<int> GetUsageCountByUserAsync(Guid userId, Guid couponId, CancellationToken cancellationToken = default);
    }
}
