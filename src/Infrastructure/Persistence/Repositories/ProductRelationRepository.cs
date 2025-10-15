using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductRelationRepository : IProductRelationRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRelationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<ProductRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.ProductRelations
                .AsNoTracking()
                .Include(pr => pr.Product)
                .Include(pr => pr.RelatedProduct)
                .FirstOrDefaultAsync(pr => pr.Id == id && !pr.Deleted, cancellationToken);

        public Task<List<ProductRelation>> GetAllAsync(CancellationToken cancellationToken = default)
            => _context.ProductRelations
                .AsNoTracking()
                .Include(pr => pr.Product)
                .Include(pr => pr.RelatedProduct)
                .Where(pr => !pr.Deleted)
                .OrderBy(pr => pr.Weight)
                .ToListAsync(cancellationToken);

        public Task<List<ProductRelation>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _context.ProductRelations
                .AsNoTracking()
                .Include(pr => pr.RelatedProduct)
                .Where(pr => pr.ProductId == productId && !pr.Deleted && pr.IsActive)
                .OrderByDescending(pr => pr.Weight)
                .ToListAsync(cancellationToken);

        public Task<List<ProductRelation>> GetRelatedProductsAsync(Guid productId, string relationType, int limit = 10, CancellationToken cancellationToken = default)
            => _context.ProductRelations
                .AsNoTracking()
                .Include(pr => pr.RelatedProduct)
                .Where(pr => pr.ProductId == productId && 
                           pr.RelationType == relationType && 
                           !pr.Deleted && 
                           pr.IsActive)
                .OrderByDescending(pr => pr.Weight)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<bool> ExistsAsync(Guid productId, Guid relatedProductId, CancellationToken cancellationToken = default)
            => _context.ProductRelations
                .AnyAsync(pr => pr.ProductId == productId && 
                              pr.RelatedProductId == relatedProductId && 
                              !pr.Deleted, cancellationToken);

        public async Task AddAsync(ProductRelation productRelation, CancellationToken cancellationToken = default)
        {
            await _context.ProductRelations.AddAsync(productRelation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductRelation productRelation, CancellationToken cancellationToken = default)
        {
            _context.ProductRelations.Update(productRelation);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var productRelation = await _context.ProductRelations.FindAsync(new object[] { id }, cancellationToken);
            if (productRelation != null)
            {
                productRelation.Delete(null);
                _context.ProductRelations.Update(productRelation);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
