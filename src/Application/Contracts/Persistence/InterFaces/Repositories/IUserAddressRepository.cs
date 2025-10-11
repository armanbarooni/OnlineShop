using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IUserAddressRepository
    {
        Task<UserAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserAddress?> GetBillingAddressAsync(Guid userId, CancellationToken cancellationToken);
        Task<UserAddress?> GetShippingAddressAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserAddress>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(UserAddress userAddress, CancellationToken cancellationToken);
        Task UpdateAsync(UserAddress userAddress, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task SetAsDefaultAsync(Guid userId, Guid addressId, CancellationToken cancellationToken);
    }
}
