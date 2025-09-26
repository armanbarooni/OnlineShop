using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}
