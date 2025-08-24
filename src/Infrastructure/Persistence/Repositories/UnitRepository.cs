
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Infrastructure.Persistence;

public class UnitRepository : IUnitRepository
{
    private readonly ApplicationDbContext _context;

    public UnitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<List<Unit>> GetAllAsync(CancellationToken cancellationToken = default)
        => _context.Units.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        await _context.Units.AddAsync(unit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _context.Units.Update(unit);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _context.Units.Remove(unit);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Units.AnyAsync(u => u.Name == name);
    }
}
