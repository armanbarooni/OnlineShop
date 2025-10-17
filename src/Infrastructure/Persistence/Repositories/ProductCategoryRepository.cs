using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
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
            productCategory.Delete(null);
            _context.ProductCategories.Update(productCategory);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null && !pc.Deleted)
                .Include(pc => pc.SubCategories)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == parentId && !pc.Deleted)
                .Include(pc => pc.SubCategories)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null && !pc.Deleted)
                .Include(pc => pc.SubCategories)
                    .ThenInclude(sc => sc.SubCategories)
                .ToListAsync(cancellationToken);
        }
    }
}
