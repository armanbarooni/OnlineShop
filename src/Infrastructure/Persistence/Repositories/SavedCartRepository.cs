using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class SavedCartRepository : ISavedCartRepository
    {
        private readonly ApplicationDbContext _context;

        public SavedCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavedCart?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.SavedCarts
                .AsNoTracking()
                .Include(sc => sc.User)
                .Include(sc => sc.Cart)
                .FirstOrDefaultAsync(sc => sc.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<SavedCart>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.SavedCarts
                .AsNoTracking()
                .Include(sc => sc.User)
                .Include(sc => sc.Cart)
                .Where(sc => sc.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SavedCart>> GetFavoriteCartsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.SavedCarts
                .AsNoTracking()
                .Include(sc => sc.User)
                .Include(sc => sc.Cart)
                .Where(sc => sc.UserId == userId && sc.IsFavorite)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SavedCart>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.SavedCarts
                .AsNoTracking()
                .Include(sc => sc.User)
                .Include(sc => sc.Cart)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(SavedCart savedCart, CancellationToken cancellationToken)
        {
            await _context.SavedCarts.AddAsync(savedCart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(SavedCart savedCart, CancellationToken cancellationToken)
        {
            _context.SavedCarts.Update(savedCart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var savedCart = await _context.SavedCarts.FindAsync(id, cancellationToken);
            if (savedCart != null)
            {
                savedCart.Delete(null);
                _context.SavedCarts.Update(savedCart);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RecordAccessAsync(Guid id, CancellationToken cancellationToken)
        {
            var savedCart = await _context.SavedCarts.FindAsync(id, cancellationToken);
            if (savedCart != null)
            {
                savedCart.RecordAccess();
                _context.SavedCarts.Update(savedCart);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
