using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Product?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken);
        Task<Product?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Product>> GetAllWithIncludesAsync(CancellationToken cancellationToken);
        Task<IEnumerable<Product>> GetByIdsWithIncludesAsync(List<Guid> ids, CancellationToken cancellationToken);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
        Task<IQueryable<Product>> GetQueryableWithIncludesAsync(CancellationToken cancellationToken);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}