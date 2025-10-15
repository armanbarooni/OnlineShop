using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ProductReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .Where(pr => pr.ProductId == productId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductReview>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .Where(pr => pr.UserId == userId)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductReview>> GetApprovedReviewsAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .Where(pr => pr.ProductId == productId && pr.IsApproved)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductReview>> GetPendingReviewsAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .Where(pr => !pr.IsApproved)
                .OrderBy(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductReview>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AsNoTracking()
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken)
        {
            var reviews = await _context.ProductReviews
                .Where(pr => pr.ProductId == productId && pr.IsApproved)
                .Select(pr => pr.Rating)
                .ToListAsync(cancellationToken);

            return reviews.Any() ? reviews.Average() : 0;
        }

        public async Task<int> GetReviewCountAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .CountAsync(pr => pr.ProductId == productId && pr.IsApproved, cancellationToken);
        }

        public async Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductReviews
                .AnyAsync(pr => pr.UserId == userId && pr.ProductId == productId, cancellationToken);
        }

        public async Task AddAsync(ProductReview productReview, CancellationToken cancellationToken)
        {
            await _context.ProductReviews.AddAsync(productReview, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductReview productReview, CancellationToken cancellationToken)
        {
            _context.ProductReviews.Update(productReview);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var productReview = await _context.ProductReviews.FindAsync(id, cancellationToken);
            if (productReview != null)
            {
                productReview.Delete(null);
                _context.ProductReviews.Update(productReview);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
