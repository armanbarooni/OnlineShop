using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories
{
    public interface IMahakMappingRepository
    {
        Task<MahakMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<MahakMapping?> GetByLocalEntityIdAsync(string entityType, Guid localEntityId, CancellationToken cancellationToken);
        Task<MahakMapping?> GetByMahakEntityIdAsync(string entityType, int mahakEntityId, CancellationToken cancellationToken);
        Task<IEnumerable<MahakMapping>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken);
        Task<IEnumerable<MahakMapping>> GetActiveMappingsAsync(CancellationToken cancellationToken);
        Task<IEnumerable<MahakMapping>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(MahakMapping mahakMapping, CancellationToken cancellationToken);
        Task UpdateAsync(MahakMapping mahakMapping, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
