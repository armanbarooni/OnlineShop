using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
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
            => _context.ProductVariants.AsNoTracking().FirstOrDefaultAsync(pv => pv.Id == id && !pv.Deleted, cancellationToken);

        public Task<ProductVariant?> GetByMahakIdAsync(int mahakId, CancellationToken cancellationToken = default)
            => _context.ProductVariants.AsNoTracking().FirstOrDefaultAsync(pv => pv.MahakId == mahakId && !pv.Deleted, cancellationToken);

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
            // Avoid graph re-attachment conflicts (Product/ProductVariant already tracked in the same DbContext).
            // Update only the variant scalar state.
            var local = _context.ProductVariants.Local.FirstOrDefault(v => v.Id == variant.Id);
            if (local != null)
            {
                _context.Entry(local).CurrentValues.SetValues(variant);
                _context.Entry(local).State = EntityState.Modified;
            }
            else
            {
                _context.ProductVariants.Attach(variant);
                _context.Entry(variant).State = EntityState.Modified;
            }

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



