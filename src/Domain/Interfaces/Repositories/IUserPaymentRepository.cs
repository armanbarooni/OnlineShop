using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserPaymentRepository
    {
        Task<UserPayment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<UserPayment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<UserPayment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken);
        Task<IEnumerable<UserPayment>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<IEnumerable<UserPayment>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(UserPayment userPayment, CancellationToken cancellationToken);
        Task UpdateAsync(UserPayment userPayment, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
