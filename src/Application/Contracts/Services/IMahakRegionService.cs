using OnlineShop.Application.DTOs.Mahak;

namespace OnlineShop.Application.Contracts.Services
{
    public interface IMahakRegionService
    {
        Task<IReadOnlyList<MahakRegionDto>> GetRegionsAsync(CancellationToken cancellationToken);
    }
}
