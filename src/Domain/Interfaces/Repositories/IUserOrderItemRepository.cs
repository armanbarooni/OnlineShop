using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserOrderItemRepository
    {
        Task<UserOrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrderItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrderItem>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(UserOrderItem userOrderItem, CancellationToken cancellationToken);
        Task UpdateAsync(UserOrderItem userOrderItem, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    }
}
