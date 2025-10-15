using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class MahakMappingRepository : IMahakMappingRepository
    {
        private readonly ApplicationDbContext _context;

        public MahakMappingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MahakMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(mm => mm.Id == id, cancellationToken);
        }

        public async Task<MahakMapping?> GetByLocalEntityIdAsync(string entityType, Guid localEntityId, CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(mm => mm.EntityType == entityType && mm.LocalEntityId == localEntityId, cancellationToken);
        }

        public async Task<MahakMapping?> GetByMahakEntityIdAsync(string entityType, int mahakEntityId, CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(mm => mm.EntityType == entityType && mm.MahakEntityId == mahakEntityId, cancellationToken);
        }

        public async Task<IEnumerable<MahakMapping>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .Where(mm => mm.EntityType == entityType)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakMapping>> GetActiveMappingsAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .Where(mm => mm.MappingStatus == "Active")
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MahakMapping>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.MahakMappings
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(MahakMapping mahakMapping, CancellationToken cancellationToken)
        {
            await _context.MahakMappings.AddAsync(mahakMapping, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(MahakMapping mahakMapping, CancellationToken cancellationToken)
        {
            _context.MahakMappings.Update(mahakMapping);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var mahakMapping = await _context.MahakMappings.FindAsync(id, cancellationToken);
            if (mahakMapping != null)
            {
                _context.MahakMappings.Remove(mahakMapping);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
