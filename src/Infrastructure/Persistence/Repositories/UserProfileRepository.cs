using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.Id == id, cancellationToken);
        }

        public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(up => up.User)
                .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.UserProfiles
                .AsNoTracking()
                .Include(up => up.User)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNationalCodeAsync(string nationalCode, CancellationToken cancellationToken)
        {
            return await _context.UserProfiles
                .AnyAsync(up => up.NationalCode == nationalCode, cancellationToken);
        }

        public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken)
        {
            await _context.UserProfiles.AddAsync(userProfile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken)
        {
            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id, cancellationToken);
            if (userProfile != null)
            {
                userProfile.Delete(null);
                _context.UserProfiles.Update(userProfile);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
