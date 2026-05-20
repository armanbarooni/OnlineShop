using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using System.Collections.Generic;

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
            var visibleCategoryIds = await GetVisibleCategoryIdsAsync(cancellationToken);

            return await _context.ProductCategories
                .Where(pc => !pc.Deleted && visibleCategoryIds.Contains(pc.Id))
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
            var visibleCategoryIds = await GetVisibleCategoryIdsAsync(cancellationToken);

            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null && !pc.Deleted && visibleCategoryIds.Contains(pc.Id))
                .Include(pc => pc.SubCategories.Where(sc => !sc.Deleted && visibleCategoryIds.Contains(sc.Id)))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            var visibleCategoryIds = await GetVisibleCategoryIdsAsync(cancellationToken);

            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == parentId && !pc.Deleted && visibleCategoryIds.Contains(pc.Id))
                .Include(pc => pc.SubCategories.Where(sc => !sc.Deleted && visibleCategoryIds.Contains(sc.Id)))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
        {
            var visibleCategoryIds = await GetVisibleCategoryIdsAsync(cancellationToken);

            return await _context.ProductCategories
                .Where(pc => pc.ParentCategoryId == null && !pc.Deleted && visibleCategoryIds.Contains(pc.Id))
                .Include(pc => pc.SubCategories.Where(sc => !sc.Deleted && visibleCategoryIds.Contains(sc.Id)))
                    .ThenInclude(sc => sc.SubCategories.Where(ssc => !ssc.Deleted && visibleCategoryIds.Contains(ssc.Id)))
                .ToListAsync(cancellationToken);
        }

        private async Task<HashSet<Guid>> GetVisibleCategoryIdsAsync(CancellationToken cancellationToken)
        {
            var categories = await _context.ProductCategories
                .Where(pc => !pc.Deleted)
                .Select(pc => new { pc.Id, pc.ParentCategoryId })
                .ToListAsync(cancellationToken);

            var stockedCategoryIds = await _context.Products
                .Where(p =>
                    p.IsActive &&
                    p.CategoryId.HasValue &&
                    (p.StockQuantity > 0 || p.ProductVariants.Any(v => v.IsAvailable && v.StockQuantity > 0)))
                .Select(p => p.CategoryId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var parentByCategoryId = categories.ToDictionary(c => c.Id, c => c.ParentCategoryId);
            var visibleCategoryIds = new HashSet<Guid>(stockedCategoryIds);

            foreach (var categoryId in stockedCategoryIds)
            {
                var currentId = categoryId;
                while (parentByCategoryId.TryGetValue(currentId, out var parentId) && parentId.HasValue)
                {
                    if (!visibleCategoryIds.Add(parentId.Value))
                    {
                        break;
                    }

                    currentId = parentId.Value;
                }
            }

            return visibleCategoryIds;
        }
    }
}
