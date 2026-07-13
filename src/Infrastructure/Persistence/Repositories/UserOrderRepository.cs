using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public UserOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<UserOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<IEnumerable<UserOrder>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserOrder>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .Where(o => o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserOrder>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .Where(o => o.TrackingNumber == trackingNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserOrder>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _context.UserOrders
                .CountAsync(o => o.OrderNumber.StartsWith($"ORD{today}"), cancellationToken);
            
            return $"ORD{today}{(count + 1):D4}";
        }

        public async Task AddAsync(UserOrder userOrder, CancellationToken cancellationToken)
        {
            await _context.UserOrders.AddAsync(userOrder, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserOrder userOrder, CancellationToken cancellationToken)
        {
            _context.UserOrders.Update(userOrder);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userOrder = await _context.UserOrders.FindAsync(id, cancellationToken);
            if (userOrder != null)
            {
                userOrder.Delete(null);
                _context.UserOrders.Update(userOrder);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<UserOrder?> GetByIdForMahakSyncAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id && !o.Deleted, cancellationToken);
        }

        public async Task<List<UserOrder>> GetUnsyncedOrdersAsync(CancellationToken cancellationToken)
        {
            return await _context.UserOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.User) // Include user for customer sync to Mahak
                    .ThenInclude(u => u.UserProfile)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Payments)
                .Where(o => !o.SyncedToMahak && (o.OrderStatus == "Completed" || o.OrderStatus == "Confirmed") && !o.Deleted)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UserOrder>> GetExpiredPendingOrdersAsync(int timeoutMinutes, CancellationToken cancellationToken)
        {
            var expiryTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
            return await _context.UserOrders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == "Pending" && o.CreatedAt < expiryTime && !o.Deleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserOrder?> GetByPaymentAuthorityAsync(string authority, CancellationToken cancellationToken)
        {
            // We need to find an order that has a payment with this authority (TransactionId)
            // Assuming Authority is stored in TransactionId or we add a field. 
            // In Mock/Sadad, Authority is usually the TransactionId during processing.
            
            return await _context.UserOrders
                .Include(o => o.User)
                .Include(o => o.Payments)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Payments.Any(p => p.TransactionId == authority), cancellationToken);
        }
    }
}
