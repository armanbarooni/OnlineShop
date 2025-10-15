using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakMapping;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakMapping.Command.Update
{
    public class UpdateMahakMappingCommandHandler(
        IMahakMappingRepository repository,
        IMapper mapper) : IRequestHandler<UpdateMahakMappingCommand, Result<MahakMappingDto>>
    {
        public async Task<Result<MahakMappingDto>> Handle(UpdateMahakMappingCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakMapping == null)
                return Result<MahakMappingDto>.Failure("داده‌های نگاشت محک نباید خالی باشد");

            try
            {
                var mahakMapping = await repository.GetByIdAsync(request.MahakMapping.Id, cancellationToken);
                
                if (mahakMapping == null)
                    return Result<MahakMappingDto>.Failure("نگاشت محک یافت نشد");

                mahakMapping.Update(
                    request.MahakMapping.MahakEntityId,
                    request.MahakMapping.MahakEntityCode,
                    request.MahakMapping.Notes,
                    request.MahakMapping.UpdatedBy
                );

                await repository.UpdateAsync(mahakMapping, cancellationToken);
                return Result<MahakMappingDto>.Success(mapper.Map<MahakMappingDto>(mahakMapping));
            }
            catch (Exception ex)
            {
                return Result<MahakMappingDto>.Failure($"خطا در به‌روزرسانی نگاشت محک: {ex.Message}");
            }
        }
    }
}



