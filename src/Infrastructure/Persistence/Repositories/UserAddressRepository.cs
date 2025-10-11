using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserAddressRepository : IUserAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public UserAddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.IsDefault)
                .ThenByDescending(ua => ua.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserAddress?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsDefault, cancellationToken);
        }

        public async Task<UserAddress?> GetBillingAddressAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsBillingAddress, cancellationToken);
        }

        public async Task<UserAddress?> GetShippingAddressAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsShippingAddress, cancellationToken);
        }

        public async Task<IEnumerable<UserAddress>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserAddresses
                .AsNoTracking()
                .OrderByDescending(ua => ua.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserAddress userAddress, CancellationToken cancellationToken)
        {
            await _context.UserAddresses.AddAsync(userAddress, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserAddress userAddress, CancellationToken cancellationToken)
        {
            _context.UserAddresses.Update(userAddress);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userAddress = await _context.UserAddresses.FindAsync(id, cancellationToken);
            if (userAddress != null)
            {
                userAddress.Delete(null);
                _context.UserAddresses.Update(userAddress);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetAsDefaultAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
        {
            // Remove default flag from all addresses
            var allAddresses = await _context.UserAddresses
                .Where(ua => ua.UserId == userId)
                .ToListAsync(cancellationToken);

            foreach (var address in allAddresses)
            {
                if (address.Id == addressId)
                    address.SetAsDefault();
                else
                    address.RemoveAsDefault();
            }

            _context.UserAddresses.UpdateRange(allAddresses);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
