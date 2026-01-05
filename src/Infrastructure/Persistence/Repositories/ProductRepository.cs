using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProductVariants.OrderBy(v => v.DisplayOrder))
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(p => p.ProductSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.ProductReviews)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }


        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Products.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetAllWithIncludesAsync(CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProductVariants.OrderBy(v => v.DisplayOrder))
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(p => p.ProductSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.ProductReviews)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByIdsWithIncludesAsync(List<Guid> ids, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProductVariants.OrderBy(v => v.DisplayOrder))
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(p => p.ProductSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.ProductReviews)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Products.AnyAsync(p => p.Name == name, cancellationToken);
        }

        public async Task<IQueryable<Product>> GetQueryableWithIncludesAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(_context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Unit)
                .Include(p => p.ProductImages.OrderBy(i => i.DisplayOrder))
                .Include(p => p.ProductVariants.OrderBy(v => v.DisplayOrder))
                .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
                .Include(p => p.ProductSeasons).ThenInclude(ps => ps.Season)
                .Include(p => p.ProductReviews)
                .AsQueryable());
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync(id, cancellationToken);
            if (product != null)
            {
                product.Delete(null);
                _context.Products.Update(product);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}