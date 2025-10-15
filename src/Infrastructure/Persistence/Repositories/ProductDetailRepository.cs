using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductDetailRepository : IProductDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDetail?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ProductDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(pd => pd.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ProductDetail>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductDetails
                .AsNoTracking()
                .Where(pd => pd.ProductId == productId)
                .OrderBy(pd => pd.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductDetail>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductDetails
                .AsNoTracking()
                .OrderBy(pd => pd.ProductId)
                .ThenBy(pd => pd.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductDetail productDetail, CancellationToken cancellationToken)
        {
            await _context.ProductDetails.AddAsync(productDetail, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductDetail productDetail, CancellationToken cancellationToken)
        {
            _context.ProductDetails.Update(productDetail);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var productDetail = await _context.ProductDetails.FindAsync(id, cancellationToken);
            if (productDetail != null)
            {
                productDetail.Delete(null);
                _context.ProductDetails.Update(productDetail);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var productDetails = await _context.ProductDetails
                .Where(pd => pd.ProductId == productId)
                .ToListAsync(cancellationToken);

            foreach (var detail in productDetails)
            {
                detail.Delete(null);
            }

            if (productDetails.Any())
            {
                _context.ProductDetails.UpdateRange(productDetails);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
