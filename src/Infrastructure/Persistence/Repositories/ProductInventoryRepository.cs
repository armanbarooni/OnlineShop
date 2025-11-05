using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using System.Linq;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductInventoryRepository : IProductInventoryRepository
    {
        private readonly ApplicationDbContext _context;
        // per-product locks to serialize updates and avoid oversell when running against the InMemory provider
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim> _locks
            = new System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim>();

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
            var sem = _locks.GetOrAdd(productInventory.ProductId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await sem.WaitAsync(cancellationToken);
            try
            {
                // Use a tracked entity to ensure EF has original values
                var tracked = await _context.ProductInventories.FindAsync(new object[] { productInventory.Id }, cancellationToken);
                if (tracked != null)
                {
                    // copy current values into the tracked entity
                    _context.Entry(tracked).CurrentValues.SetValues(productInventory);
                }
                else
                {
                    // No tracked entity found (new or detached) - attach the provided entity
                    _context.ProductInventories.Attach(productInventory);
                    _context.Entry(productInventory).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                sem.Release();
            }
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

        public async Task<bool> TryReserveAsync(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            var semaphore = _locks.GetOrAdd(productId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var inventoryMap = await LoadOrCreateInventoriesAsync(new[] { (productId, quantity) }, cancellationToken);
                if (!inventoryMap.TryGetValue(productId, out var inventories) || inventories.Count == 0)
                    return false;

                if (!HasSufficientStock(inventories, quantity))
                    return false;

                ApplyReservation(inventories, quantity);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<bool> TryReserveMultipleAsync(IEnumerable<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken)
        {
            var grouped = items
                .GroupBy(i => i.ProductId)
                .Select(g => (ProductId: g.Key, Quantity: g.Sum(x => x.Quantity)))
                .ToList();

            if (grouped.Count == 0)
                return true;

            var orderedProductIds = grouped.Select(g => g.ProductId).OrderBy(id => id).ToList();
            var semaphores = orderedProductIds
                .Select(id => _locks.GetOrAdd(id, _ => new System.Threading.SemaphoreSlim(1, 1)))
                .ToList();

            foreach (var semaphore in semaphores)
            {
                await semaphore.WaitAsync(cancellationToken);
            }

            try
            {
                var inventoryMap = await LoadOrCreateInventoriesAsync(grouped, cancellationToken);

                foreach (var (productId, quantity) in grouped)
                {
                    if (!inventoryMap.TryGetValue(productId, out var inventories) || inventories.Count == 0)
                        return false;

                    if (!HasSufficientStock(inventories, quantity))
                        return false;
                }

                foreach (var (productId, quantity) in grouped)
                {
                    var inventories = inventoryMap[productId];
                    ApplyReservation(inventories, quantity);
                }

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            finally
            {
                for (int i = semaphores.Count - 1; i >= 0; i--)
                {
                    semaphores[i].Release();
                }
            }
        }

        private async Task<Dictionary<Guid, List<ProductInventory>>> LoadOrCreateInventoriesAsync(
            IEnumerable<(Guid ProductId, int Quantity)> requests,
            CancellationToken cancellationToken)
        {
            var productIds = requests.Select(r => r.ProductId).Distinct().ToList();

            var existingInventories = await _context.ProductInventories
                .Where(pi => productIds.Contains(pi.ProductId) && !pi.Deleted)
                .ToListAsync(cancellationToken);

            var inventoryMap = existingInventories
                .GroupBy(pi => pi.ProductId)
                .ToDictionary(g => g.Key, g => g.OrderBy(inv => inv.CreatedAt).ToList());

            foreach (var (productId, _) in requests)
            {
                if (inventoryMap.ContainsKey(productId))
                    continue;

                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

                if (product == null)
                {
                    inventoryMap[productId] = new List<ProductInventory>();
                    continue;
                }

                var newInventory = ProductInventory.Create(productId, product.StockQuantity);
                await _context.ProductInventories.AddAsync(newInventory, cancellationToken);
                inventoryMap[productId] = new List<ProductInventory> { newInventory };
            }

            return inventoryMap;
        }

        private static bool HasSufficientStock(IEnumerable<ProductInventory> inventories, int requiredQuantity)
        {
            var totalAvailable = inventories.Sum(inv => inv.GetAvailableStock());
            return totalAvailable >= requiredQuantity;
        }

        private static void ApplyReservation(IEnumerable<ProductInventory> inventories, int requiredQuantity)
        {
            var remaining = requiredQuantity;
            foreach (var inventory in inventories)
            {
                var available = inventory.GetAvailableStock();
                if (available <= 0) continue;

                var take = Math.Min(available, remaining);
                inventory.ReserveQuantity(take);
                remaining -= take;

                if (remaining <= 0)
                    break;
            }
        }
    }
}
