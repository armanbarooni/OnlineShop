using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class SeasonRepository : ISeasonRepository
    {
        private readonly ApplicationDbContext _context;

        public SeasonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Season?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && !s.Deleted, cancellationToken);

        public Task<List<Season>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.Seasons.AsNoTracking().Where(s => !s.Deleted).OrderBy(s => s.Name).ToListAsync(cancellationToken);

        public Task<Season?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
            => _context.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.Code == code && !s.Deleted, cancellationToken);

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.Seasons.AnyAsync(s => s.Name == name && !s.Deleted, cancellationToken);

        public async Task AddAsync(Season season, CancellationToken cancellationToken = default)
        {
            await _context.Seasons.AddAsync(season, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Season season, CancellationToken cancellationToken = default)
        {
            _context.Seasons.Update(season);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var season = await _context.Seasons.FindAsync(new object[] { id }, cancellationToken);
            if (season != null)
            {
                season.Delete(null);
                _context.Seasons.Update(season);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

