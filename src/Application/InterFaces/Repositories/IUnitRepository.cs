namespace OnlineShop.Application.Interfaces.Repositories;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Unit>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Unit unit, CancellationToken cancellationToken);
    Task UpdateAsync(Unit unit, CancellationToken cancellationToken);
    Task DeleteAsync(Unit unit, CancellationToken cancellationToken);
}
