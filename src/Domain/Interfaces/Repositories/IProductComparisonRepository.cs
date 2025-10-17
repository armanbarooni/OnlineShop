using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductComparisonRepository
    {
        Task<ProductComparison?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<ProductComparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(ProductComparison productComparison, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductComparison productComparison, CancellationToken cancellationToken = default);
        Task DeleteAsync(ProductComparison productComparison, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

