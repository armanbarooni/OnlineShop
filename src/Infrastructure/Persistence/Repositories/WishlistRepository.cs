using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wishlist?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Wishlists
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Wishlist>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Wishlists
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Wishlist?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        {
            return await _context.Wishlists
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
        }

        public async Task<IEnumerable<Wishlist>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Wishlists
                .AsNoTracking()
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        {
            return await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
        }

        public async Task AddAsync(Wishlist wishlist, CancellationToken cancellationToken)
        {
            await _context.Wishlists.AddAsync(wishlist, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Wishlist wishlist, CancellationToken cancellationToken)
        {
            _context.Wishlists.Update(wishlist);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var wishlist = await _context.Wishlists.FindAsync(id, cancellationToken);
            if (wishlist != null)
            {
                wishlist.Delete(null);
                _context.Wishlists.Update(wishlist);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

            if (wishlist != null)
            {
                wishlist.Delete(null);
                _context.Wishlists.Update(wishlist);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
