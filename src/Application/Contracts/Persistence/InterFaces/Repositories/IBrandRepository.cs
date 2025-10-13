using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Brand>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task AddAsync(Brand brand, CancellationToken cancellationToken = default);
        Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

