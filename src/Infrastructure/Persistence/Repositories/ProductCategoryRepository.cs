using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.Id == id && !pc.Deleted, cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => !pc.Deleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            await _context.ProductCategories.AddAsync(productCategory, cancellationToken);
        }

        public Task UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            _context.ProductCategories.Update(productCategory);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            productCategory.Delete(1);
            _context.ProductCategories.Update(productCategory);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
