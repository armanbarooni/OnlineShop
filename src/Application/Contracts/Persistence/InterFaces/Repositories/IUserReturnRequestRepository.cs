using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IUserReturnRequestRepository
    {
        Task<UserReturnRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<UserReturnRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserReturnRequest>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<IEnumerable<UserReturnRequest>> GetByStatusAsync(string status, CancellationToken cancellationToken);
        Task<IEnumerable<UserReturnRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<UserReturnRequest>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(UserReturnRequest userReturnRequest, CancellationToken cancellationToken);
        Task UpdateAsync(UserReturnRequest userReturnRequest, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
