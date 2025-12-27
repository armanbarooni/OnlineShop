using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly ApplicationDbContext _context;

        public OtpRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Otp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _context.Otps.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && !o.Deleted, cancellationToken);

        public Task<IEnumerable<Otp>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<Otp>>(_context.Otps.AsNoTracking().Where(o => !o.Deleted).ToList());

        public async Task AddAsync(Otp otp, CancellationToken cancellationToken = default)
        {
            await _context.Otps.AddAsync(otp, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Otp otp, CancellationToken cancellationToken = default)
        {
            _context.Otps.Update(otp);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var otp = await _context.Otps.FindAsync(new object[] { id }, cancellationToken);
            if (otp != null)
            {
                otp.Delete();
                _context.Otps.Update(otp);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Otp?> GetValidOtpByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            
            return await _context.Otps
                .Where(o => o.PhoneNumber == phoneNumber
                            && !o.IsUsed
                            && !o.Deleted
                            && o.ExpiresAt > now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Otp>> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Otps
                .Where(o => o.PhoneNumber == phoneNumber && !o.Deleted)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteExpiredOtpsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredOtps = await _context.Otps
                .Where(o => o.ExpiresAt < now || o.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var otp in expiredOtps)
            {
                otp.Delete();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task InvalidatePreviousOtpsAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var previousOtps = await _context.Otps
                .Where(o => o.PhoneNumber == phoneNumber 
                            && !o.IsUsed 
                            && !o.Deleted)
                .ToListAsync(cancellationToken);

            foreach (var otp in previousOtps)
            {
                otp.Delete();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Otp?> GetLatestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Otps
                .Where(o => o.PhoneNumber == phoneNumber && !o.Deleted)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

