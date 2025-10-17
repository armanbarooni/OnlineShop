using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductCategoryRepository
    {
        Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task DeleteAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        // Hierarchical category methods
        Task<IEnumerable<ProductCategory>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    }
}
