using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && !b.Deleted, cancellationToken);

        public Task<List<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.Brands.AsNoTracking().Where(b => !b.Deleted).OrderBy(b => b.DisplayOrder).ThenBy(b => b.Name).ToListAsync(cancellationToken);

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
            => _context.Brands.AnyAsync(b => b.Name == name && !b.Deleted, cancellationToken);

        public async Task AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            await _context.Brands.AddAsync(brand, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var brand = await _context.Brands.FindAsync(new object[] { id }, cancellationToken);
            if (brand != null)
            {
                brand.Delete(null);
                _context.Brands.Update(brand);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

