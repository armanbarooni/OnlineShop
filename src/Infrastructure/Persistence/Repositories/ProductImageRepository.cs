using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductImage?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ProductImages
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductImages
                .AsNoTracking()
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductImage?> GetPrimaryImageAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductImages
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsPrimary, cancellationToken);
        }

        public async Task<IEnumerable<ProductImage>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductImages
                .AsNoTracking()
                .OrderBy(pi => pi.ProductId)
                .ThenBy(pi => pi.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductImage productImage, CancellationToken cancellationToken)
        {
            await _context.ProductImages.AddAsync(productImage, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductImage productImage, CancellationToken cancellationToken)
        {
            _context.ProductImages.Update(productImage);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var productImage = await _context.ProductImages.FindAsync(id, cancellationToken);
            if (productImage != null)
            {
                productImage.Delete(null);
                _context.ProductImages.Update(productImage);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var productImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync(cancellationToken);

            foreach (var image in productImages)
            {
                image.Delete(null);
            }

            if (productImages.Any())
            {
                _context.ProductImages.UpdateRange(productImages);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetPrimaryImageAsync(Guid productId, Guid imageId, CancellationToken cancellationToken)
        {
            // Remove primary flag from all images of this product
            var allImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync(cancellationToken);

            foreach (var image in allImages)
            {
                image.SetIsPrimary(image.Id == imageId);
            }

            _context.ProductImages.UpdateRange(allImages);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
