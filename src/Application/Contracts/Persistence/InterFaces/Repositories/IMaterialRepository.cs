using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IMaterialRepository
    {
        Task<Material?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task AddAsync(Material material, CancellationToken cancellationToken = default);
        Task UpdateAsync(Material material, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

