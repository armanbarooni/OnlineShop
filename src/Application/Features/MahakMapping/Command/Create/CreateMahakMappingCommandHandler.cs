using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.MahakMapping;

namespace OnlineShop.Application.Features.MahakMapping.Command.Create
{
    public class CreateMahakMappingCommandHandler(
        IMahakMappingRepository repository,
        IMapper mapper) : IRequestHandler<CreateMahakMappingCommand, Result<MahakMappingDto>>
    {
        public async Task<Result<MahakMappingDto>> Handle(CreateMahakMappingCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakMapping == null)
                return Result<MahakMappingDto>.Failure("داده‌های نگاشت محک نباید خالی باشد");

            try
            {
                var mahakMapping = Domain.Entities.MahakMapping.Create(
                    request.MahakMapping.EntityType,
                    request.MahakMapping.LocalEntityId,
                    request.MahakMapping.MahakEntityId,
                    request.MahakMapping.MahakEntityCode,
                    request.MahakMapping.Notes
                );

                await repository.AddAsync(mahakMapping, cancellationToken);
                return Result<MahakMappingDto>.Success(mapper.Map<MahakMappingDto>(mahakMapping));
            }
            catch (Exception ex)
            {
                return Result<MahakMappingDto>.Failure($"خطا در ایجاد نگاشت محک: {ex.Message}");
            }
        }
    }
}

