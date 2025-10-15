using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IOrderStatusHistoryRepository : IGenericRepository<OrderStatusHistory>
    {
        /// <summary>
        /// Get all status history for an order
        /// </summary>
        Task<List<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get status history for an order ordered by date
        /// </summary>
        Task<List<OrderStatusHistory>> GetOrderTimelineAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the latest status for an order
        /// </summary>
        Task<OrderStatusHistory?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get status history by status
        /// </summary>
        Task<List<OrderStatusHistory>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get status history by date range
        /// </summary>
        Task<List<OrderStatusHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
