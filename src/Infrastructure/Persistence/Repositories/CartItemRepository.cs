using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly ApplicationDbContext _context;

        public CartItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.CartId == cartId)
                .ToListAsync(cancellationToken);
        }

        public async Task<CartItem?> GetByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId, cancellationToken);
        }

        public async Task<IEnumerable<CartItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(CartItem cartItem, CancellationToken cancellationToken)
        {
            await _context.CartItems.AddAsync(cartItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems.FindAsync(id, cancellationToken);
            if (cartItem != null)
            {
                cartItem.Delete(null);
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteByCartIdAsync(Guid cartId, CancellationToken cancellationToken)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
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

        public async Task DeleteByCartAndProductAsync(Guid cartId, Guid productId, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId, cancellationToken);

            if (cartItem != null)
            {
                cartItem.Delete(null);
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
