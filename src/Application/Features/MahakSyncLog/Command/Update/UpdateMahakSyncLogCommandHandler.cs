using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Command.Update
{
    public class UpdateMahakSyncLogCommandHandler(
        IMahakSyncLogRepository repository,
        IMapper mapper) : IRequestHandler<UpdateMahakSyncLogCommand, Result<MahakSyncLogDto>>
    {
        public async Task<Result<MahakSyncLogDto>> Handle(UpdateMahakSyncLogCommand request, CancellationToken cancellationToken)
        {
            if (request.MahakSyncLog == null)
                return Result<MahakSyncLogDto>.Failure("داده‌های لاگ سینک محک نباید خالی باشد");

            try
            {
                var mahakSyncLog = await repository.GetByIdAsync(request.MahakSyncLog.Id, cancellationToken);
                
                if (mahakSyncLog == null)
                    return Result<MahakSyncLogDto>.Failure("لاگ سینک محک یافت نشد");

                mahakSyncLog.Update(
                    request.MahakSyncLog.EntityType,
                    request.MahakSyncLog.SyncType,
                    request.MahakSyncLog.SyncStatus,
                    request.MahakSyncLog.RecordsProcessed,
                    request.MahakSyncLog.UpdatedBy
                );

                await repository.UpdateAsync(mahakSyncLog, cancellationToken);
                return Result<MahakSyncLogDto>.Success(mapper.Map<MahakSyncLogDto>(mahakSyncLog));
            }
            catch (Exception ex)
            {
                return Result<MahakSyncLogDto>.Failure($"خطا در به‌روزرسانی لاگ سینک محک: {ex.Message}");
            }
        }
    }
}

