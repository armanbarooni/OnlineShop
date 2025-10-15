using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserOrderRepository
    {
        Task<UserOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<UserOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrder>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrder>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrder>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken);
        Task<IEnumerable<UserOrder>> GetAllAsync(CancellationToken cancellationToken);
        Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken);
        Task AddAsync(UserOrder userOrder, CancellationToken cancellationToken);
        Task UpdateAsync(UserOrder userOrder, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
