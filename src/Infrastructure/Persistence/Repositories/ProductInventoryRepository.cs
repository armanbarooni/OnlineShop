using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;
using System.Linq;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class ProductInventoryRepository : IProductInventoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductInventoryRepository> _logger;
        // per-product locks to serialize updates and avoid oversell when running against the InMemory provider
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim> _locks
            = new System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim>();

        public ProductInventoryRepository(
            ApplicationDbContext context,
            ILogger<ProductInventoryRepository> logger)
        {
            _context = context;
            _logger = logger;
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
                var inventoryMap = await LoadOrCreateInventoriesAsync(Guid.Empty, new[] { (productId, quantity) }, cancellationToken);
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

        public async Task<bool> TryReserveMultipleAsync(
            Guid orderId,
            IEnumerable<(Guid ProductId, Guid? VariantId, int Quantity)> items,
            CancellationToken cancellationToken)
        {
            var requests = items.ToList();
            var variantRequests = requests
                .Where(i => i.VariantId.HasValue && i.VariantId.Value != Guid.Empty)
                .GroupBy(i => i.VariantId!.Value)
                .Select(g => (VariantId: g.Key, Quantity: g.Sum(i => i.Quantity)))
                .ToList();

            if (variantRequests.Count > 0)
            {
                var variantIds = variantRequests.Select(i => i.VariantId).ToList();
                var variants = await _context.ProductVariants
                    .Where(v => variantIds.Contains(v.Id) && !v.Deleted)
                    .ToDictionaryAsync(v => v.Id, cancellationToken);
                var cutoff = DateTime.UtcNow.AddMinutes(-10);
                var activeReservations = await _context.UserOrderItems.AsNoTracking()
                    .Where(i => i.VariantId.HasValue && variantIds.Contains(i.VariantId.Value)
                        && i.OrderId != orderId
                        && i.Order.OrderStatus == "Pending" && i.Order.CreatedAt >= cutoff)
                    .GroupBy(i => i.VariantId!.Value)
                    .Select(g => new { VariantId = g.Key, Quantity = g.Sum(i => i.Quantity) })
                    .ToDictionaryAsync(i => i.VariantId, i => i.Quantity, cancellationToken);

                foreach (var request in variantRequests)
                {
                    if (!variants.TryGetValue(request.VariantId, out var variant))
                        return false;
                    variant.ReconcileReservedQuantity(activeReservations.GetValueOrDefault(request.VariantId));
                    if (!variant.IsAvailable || variant.GetAvailableStock() < request.Quantity)
                    {
                        _logger.LogError("Variant reservation rejected for {VariantId}. Requested: {Requested}, stock: {Stock}, reserved: {Reserved}",
                            request.VariantId, request.Quantity, variant.StockQuantity, variant.ReservedQuantity);
                        return false;
                    }
                }

                foreach (var request in variantRequests)
                    variants[request.VariantId].ReserveQuantity(request.Quantity);
            }

            var productRequests = requests
                .Where(i => !i.VariantId.HasValue || i.VariantId.Value == Guid.Empty)
                .Select(i => (i.ProductId, i.Quantity))
                .ToList();
            if (productRequests.Count > 0 && !await TryReserveProductsAsync(orderId, productRequests, cancellationToken))
                return false;

            if (productRequests.Count == 0)
                await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task<bool> TryReserveProductsAsync(Guid orderId, IEnumerable<(Guid ProductId, int Quantity)> items, CancellationToken cancellationToken)
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
                var inventoryMap = await LoadOrCreateInventoriesAsync(orderId, grouped, cancellationToken);

                foreach (var (productId, quantity) in grouped)
                {
                    if (!inventoryMap.TryGetValue(productId, out var inventories) || inventories.Count == 0)
                    {
                        _logger.LogError(
                            "Inventory reservation rejected because product {ProductId} has no inventory source. Requested: {RequestedQuantity}",
                            productId,
                            quantity);
                        return false;
                    }

                    if (!HasSufficientStock(inventories, quantity))
                    {
                        _logger.LogError(
                            "Inventory reservation rejected for product {ProductId}. Requested: {RequestedQuantity}, physical: {PhysicalQuantity}, reserved: {ReservedQuantity}, available: {AvailableQuantity}",
                            productId,
                            quantity,
                            inventories.Sum(i => i.AvailableQuantity),
                            inventories.Sum(i => i.ReservedQuantity),
                            inventories.Sum(i => i.GetAvailableStock()));
                        return false;
                    }
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

        public async Task CommitReservationsAsync(
            IEnumerable<(Guid ProductId, Guid? VariantId, int Quantity)> items,
            DateTime soldAt,
            CancellationToken cancellationToken)
        {
            var requests = items.ToList();
            foreach (var group in requests.Where(i => i.VariantId.HasValue && i.VariantId.Value != Guid.Empty)
                         .GroupBy(i => i.VariantId!.Value))
            {
                var variant = await _context.ProductVariants.FirstAsync(v => v.Id == group.Key, cancellationToken);
                variant.CommitReservedQuantity(group.Sum(i => i.Quantity), soldAt);
            }

            foreach (var group in requests.Where(i => !i.VariantId.HasValue || i.VariantId.Value == Guid.Empty)
                         .GroupBy(i => i.ProductId))
            {
                var inventory = await _context.ProductInventories.FirstAsync(i => i.ProductId == group.Key, cancellationToken);
                inventory.CommitSale(group.Sum(i => i.Quantity), soldAt);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ReleaseReservationsAsync(
            IEnumerable<(Guid ProductId, Guid? VariantId, int Quantity)> items,
            CancellationToken cancellationToken)
        {
            var requests = items.ToList();
            foreach (var group in requests.Where(i => i.VariantId.HasValue && i.VariantId.Value != Guid.Empty)
                         .GroupBy(i => i.VariantId!.Value))
            {
                var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == group.Key, cancellationToken);
                variant?.ReleaseReservedQuantity(group.Sum(i => i.Quantity));
            }

            foreach (var group in requests.Where(i => !i.VariantId.HasValue || i.VariantId.Value == Guid.Empty)
                         .GroupBy(i => i.ProductId))
            {
                var inventory = await _context.ProductInventories.FirstOrDefaultAsync(i => i.ProductId == group.Key, cancellationToken);
                if (inventory != null)
                    inventory.SetReservedQuantity(Math.Max(0, inventory.ReservedQuantity - group.Sum(i => i.Quantity)));
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Dictionary<Guid, List<ProductInventory>>> LoadOrCreateInventoriesAsync(
            Guid orderId,
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

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Only pending orders inside the payment window represent real reservations.
            // Rebuilding this value removes reservations left behind when checkout failed
            // after reserving stock but before creating its order and items.
            var reservationCutoff = DateTime.UtcNow.AddMinutes(-10);
            var activeReservations = await _context.UserOrderItems
                .AsNoTracking()
                .Where(item => productIds.Contains(item.ProductId)
                    && item.OrderId != orderId
                    && item.Order.OrderStatus == "Pending"
                    && item.Order.CreatedAt >= reservationCutoff)
                .GroupBy(item => item.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                })
                .ToDictionaryAsync(item => item.ProductId, item => item.Quantity, cancellationToken);

            foreach (var (productId, _) in requests)
            {
                if (!products.TryGetValue(productId, out var product))
                {
                    inventoryMap[productId] = new List<ProductInventory>();
                    continue;
                }

                var reservedQuantity = activeReservations.GetValueOrDefault(productId);

                if (inventoryMap.TryGetValue(productId, out var inventories) && inventories.Count > 0)
                {
                    var canonicalInventory = inventories[0];
                    canonicalInventory.SetAvailableQuantity(product.StockQuantity);
                    canonicalInventory.SetReservedQuantity(reservedQuantity);

                    // Older data may contain more than one inventory row for a product. Keep one
                    // authoritative row so stale duplicates cannot inflate or block stock checks.
                    foreach (var duplicate in inventories.Skip(1))
                    {
                        duplicate.SetAvailableQuantity(0);
                        duplicate.SetReservedQuantity(0);
                        duplicate.Delete(null);
                    }

                    inventoryMap[productId] = new List<ProductInventory> { canonicalInventory };
                    continue;
                }

                var newInventory = ProductInventory.Create(
                    productId,
                    product.StockQuantity,
                    reservedQuantity);
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
