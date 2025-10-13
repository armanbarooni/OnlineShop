using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface ISeasonRepository
    {
        Task<Season?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Season>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Season?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task AddAsync(Season season, CancellationToken cancellationToken = default);
        Task UpdateAsync(Season season, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

