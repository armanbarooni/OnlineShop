using OnlineShop.Domain.Entites;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Unit>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Unit unit, CancellationToken cancellationToken);
    Task UpdateAsync(Unit unit, CancellationToken cancellationToken);
    Task DeleteAsync(Unit unit, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
}