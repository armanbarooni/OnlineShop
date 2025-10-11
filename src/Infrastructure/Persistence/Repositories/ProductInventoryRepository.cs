using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductInventoryRepository : IProductInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductInventoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductInventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ProductInventories
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.Id == id, cancellationToken);
        }

        public async Task<ProductInventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.ProductInventories
                .AsNoTracking()
                .FirstOrDefaultAsync(pi => pi.ProductId == productId, cancellationToken);
        }

        public async Task<IEnumerable<ProductInventory>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductInventories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductInventory>> GetLowStockItemsAsync(int threshold, CancellationToken cancellationToken)
        {
            return await _context.ProductInventories
                .AsNoTracking()
                .Where(pi => pi.AvailableQuantity <= threshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProductInventory>> GetOutOfStockItemsAsync(CancellationToken cancellationToken)
        {
            return await _context.ProductInventories
                .AsNoTracking()
                .Where(pi => pi.AvailableQuantity == 0)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            await _context.ProductInventories.AddAsync(productInventory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            _context.ProductInventories.Update(productInventory);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var productInventory = await _context.ProductInventories.FindAsync(id, cancellationToken);
            if (productInventory != null)
            {
                productInventory.Delete(null);
                _context.ProductInventories.Update(productInventory);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateStockAsync(Guid productId, int availableQuantity, int reservedQuantity, int soldQuantity, CancellationToken cancellationToken)
        {
            var productInventory = await GetByProductIdAsync(productId, cancellationToken);
            if (productInventory != null)
            {
                productInventory.UpdateInventory(availableQuantity, reservedQuantity, soldQuantity, null, null, null);
                await UpdateAsync(productInventory, cancellationToken);
            }
        }
    }
}
