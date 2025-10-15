using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive && !c.IsExpired(), cancellationToken);
        }

        public async Task<Cart?> GetCartBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive && !c.IsExpired(), cancellationToken);
        }

        public async Task<IEnumerable<Cart>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            return await _context.Carts
                .AsNoTracking()
                .Where(c => c.IsActive && c.ExpiresAt.HasValue && c.ExpiresAt.Value <= now)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cart>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.CartId == cartId && !ci.Deleted)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(id, cancellationToken);
            if (cart != null)
            {
                cart.Delete(null);
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId && !ci.Deleted)
                .ToListAsync(cancellationToken);

            foreach (var item in cartItems)
            {
                item.Delete(null);
            }

            if (cartItems.Any())
            {
                _context.CartItems.UpdateRange(cartItems);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeactivateExpiredCartsAsync(CancellationToken cancellationToken)
        {
            var expiredCarts = await GetExpiredCartsAsync(cancellationToken);
            
            foreach (var cart in expiredCarts)
            {
                cart.Deactivate();
            }

            if (expiredCarts.Any())
            {
                _context.Carts.UpdateRange(expiredCarts);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
