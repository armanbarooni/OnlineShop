using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductComparisonRepository : IProductComparisonRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductComparisonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductComparison?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProductComparison>()
                .FirstOrDefaultAsync(pc => pc.UserId == userId, cancellationToken);
        }

        public async Task<ProductComparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProductComparison>()
                .FirstOrDefaultAsync(pc => pc.Id == id, cancellationToken);
        }

        public async Task AddAsync(ProductComparison productComparison, CancellationToken cancellationToken = default)
        {
            await _context.Set<ProductComparison>().AddAsync(productComparison, cancellationToken);
        }

        public Task UpdateAsync(ProductComparison productComparison, CancellationToken cancellationToken = default)
        {
            _context.Set<ProductComparison>().Update(productComparison);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ProductComparison productComparison, CancellationToken cancellationToken = default)
        {
            _context.Set<ProductComparison>().Remove(productComparison);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

