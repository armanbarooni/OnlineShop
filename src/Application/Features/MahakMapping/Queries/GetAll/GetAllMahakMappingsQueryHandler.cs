using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakMapping;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakMapping.Queries.GetAll
{
    public class GetAllMahakMappingsQueryHandler(
        IMahakMappingRepository repository,
        IMapper mapper) : IRequestHandler<GetAllMahakMappingsQuery, Result<List<MahakMappingDto>>>
    {
        public async Task<Result<List<MahakMappingDto>>> Handle(GetAllMahakMappingsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakMappings = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<MahakMappingDto>>(mahakMappings);
                return Result<List<MahakMappingDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<MahakMappingDto>>.Failure($"خطا در دریافت لیست نگاشت‌های محک: {ex.Message}");
            }
        }
    }
}



