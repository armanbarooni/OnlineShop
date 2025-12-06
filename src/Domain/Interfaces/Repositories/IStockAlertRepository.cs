using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IStockAlertRepository : IGenericRepository<StockAlert>
    {
        Task<List<StockAlert>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<List<StockAlert>> GetByProductVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken = default);
        Task<List<StockAlert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<StockAlert>> GetUnnotifiedAlertsAsync(CancellationToken cancellationToken = default);
        Task<List<StockAlert>> GetExpiredAlertsAsync(int daysToExpire = 90, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid productId, Guid? productVariantId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<StockAlert>> GetAlertsForProductAsync(Guid productId, Guid? productVariantId, CancellationToken cancellationToken = default);
    }
}
