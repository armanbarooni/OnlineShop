using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<ProductVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.ProductVariants.AsNoTracking().Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.Id == id && !pv.Deleted, cancellationToken);

        public Task<List<ProductVariant>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.ProductVariants.AsNoTracking().Where(pv => !pv.Deleted).OrderBy(pv => pv.DisplayOrder).ToListAsync(cancellationToken);

        public Task<List<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _context.ProductVariants.AsNoTracking().Where(pv => pv.ProductId == productId && !pv.Deleted).OrderBy(pv => pv.DisplayOrder).ToListAsync(cancellationToken);

        public Task<ProductVariant?> GetBySKUAsync(string sku, CancellationToken cancellationToken = default)
            => _context.ProductVariants.AsNoTracking().FirstOrDefaultAsync(pv => pv.SKU == sku && !pv.Deleted, cancellationToken);

        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
            => _context.ProductVariants.AnyAsync(pv => pv.SKU == sku && !pv.Deleted, cancellationToken);

        public async Task AddAsync(ProductVariant variant, CancellationToken cancellationToken = default)
        {
            await _context.ProductVariants.AddAsync(variant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductVariant variant, CancellationToken cancellationToken = default)
        {
            _context.ProductVariants.Update(variant);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var variant = await _context.ProductVariants.FindAsync(new object[] { id }, cancellationToken);
            if (variant != null)
            {
                variant.Delete(null);
                _context.ProductVariants.Update(variant);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

