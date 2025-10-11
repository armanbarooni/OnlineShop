using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IProductReviewRepository
    {
        Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<ProductReview>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductReview>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductReview>> GetApprovedReviewsAsync(Guid productId, CancellationToken cancellationToken);
        Task<IEnumerable<ProductReview>> GetPendingReviewsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<ProductReview>> GetAllAsync(CancellationToken cancellationToken);
        Task<double> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken);
        Task<int> GetReviewCountAsync(Guid productId, CancellationToken cancellationToken);
        Task<bool> HasUserReviewedProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken);
        Task AddAsync(ProductReview productReview, CancellationToken cancellationToken);
        Task UpdateAsync(ProductReview productReview, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
