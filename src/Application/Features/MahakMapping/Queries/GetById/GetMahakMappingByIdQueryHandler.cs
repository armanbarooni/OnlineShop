using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakMapping;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakMapping.Queries.GetById
{
    public class GetMahakMappingByIdQueryHandler(
        IMahakMappingRepository repository,
        IMapper mapper) : IRequestHandler<GetMahakMappingByIdQuery, Result<MahakMappingDto>>
    {
        public async Task<Result<MahakMappingDto>> Handle(GetMahakMappingByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakMapping = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakMapping == null)
                    return Result<MahakMappingDto>.Failure("نگاشت محک یافت نشد");

                var dto = mapper.Map<MahakMappingDto>(mahakMapping);
                return Result<MahakMappingDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<MahakMappingDto>.Failure($"خطا در دریافت نگاشت محک: {ex.Message}");
            }
        }
    }
}



