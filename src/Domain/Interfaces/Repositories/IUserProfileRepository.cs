using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ExistsByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken);
        Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken);
        Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
