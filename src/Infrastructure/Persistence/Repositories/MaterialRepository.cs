using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public MaterialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Material?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id && !m.Deleted, cancellationToken);

        public Task<List<Material>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.Materials.AsNoTracking().Where(m => !m.Deleted).OrderBy(m => m.Name).ToListAsync(cancellationToken);

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.Materials.AnyAsync(m => m.Name == name && !m.Deleted, cancellationToken);

        public async Task AddAsync(Material material, CancellationToken cancellationToken = default)
        {
            await _context.Materials.AddAsync(material, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            _context.Materials.Update(material);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var material = await _context.Materials.FindAsync(new object[] { id }, cancellationToken);
            if (material != null)
            {
                material.Delete(null);
                _context.Materials.Update(material);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

