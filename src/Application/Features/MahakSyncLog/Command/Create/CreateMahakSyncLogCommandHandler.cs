using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakSyncLog;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakSyncLog.Command.Create
{
    public class CreateMahakSyncLogCommandHandler(
        IMahakSyncLogRepository repository,
        IMapper mapper) : IRequestHandler<CreateMahakSyncLogCommand, Result<MahakSyncLogDto>>
    {
        public async Task<Result<MahakSyncLogDto>> Handle(CreateMahakSyncLogCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakSyncLog == null)
                return Result<MahakSyncLogDto>.Failure("داده‌های لاگ سینک محک نباید خالی باشد");

            try
            {
                var mahakSyncLog = Domain.Entities.MahakSyncLog.Create(
                    request.MahakSyncLog.EntityType,
                    request.MahakSyncLog.EntityId,
                    request.MahakSyncLog.SyncType,
                    request.MahakSyncLog.SyncStatus,
                    request.MahakSyncLog.RecordsProcessed
                );

                await repository.AddAsync(mahakSyncLog, cancellationToken);
                return Result<MahakSyncLogDto>.Success(mapper.Map<MahakSyncLogDto>(mahakSyncLog));
            }
            catch (Exception ex)
            {
                return Result<MahakSyncLogDto>.Failure($"خطا در ایجاد لاگ سینک محک: {ex.Message}");
            }
        }
    }
}



