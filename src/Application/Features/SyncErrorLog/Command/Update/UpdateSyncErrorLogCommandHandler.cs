using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Update
{
    public class UpdateSyncErrorLogCommandHandler(
        ISyncErrorLogRepository repository,
        IMapper mapper) : IRequestHandler<UpdateSyncErrorLogCommand, Result<SyncErrorLogDto>>
    {
        public async Task<Result<SyncErrorLogDto>> Handle(UpdateSyncErrorLogCommand request, CancellationToken cancellationToken)
        {
            if (request.SyncErrorLog == null)
                return Result<SyncErrorLogDto>.Failure("داده‌های لاگ خطا نباید خالی باشد");

            try
            {
                var syncErrorLog = await repository.GetByIdAsync(request.SyncErrorLog.Id, cancellationToken);
                
                if (syncErrorLog == null)
                    return Result<SyncErrorLogDto>.Failure("لاگ خطا یافت نشد");

                syncErrorLog.Update(
                    request.SyncErrorLog.ErrorType,
                    request.SyncErrorLog.EntityType,
                    request.SyncErrorLog.ErrorCode,
                    request.SyncErrorLog.ErrorMessage,
                    request.SyncErrorLog.ErrorSeverity,
                    request.SyncErrorLog.UpdatedBy
                );

                await repository.UpdateAsync(syncErrorLog, cancellationToken);
                return Result<SyncErrorLogDto>.Success(mapper.Map<SyncErrorLogDto>(syncErrorLog));
            }
            catch (Exception ex)
            {
                return Result<SyncErrorLogDto>.Failure($"خطا در به‌روزرسانی لاگ خطا: {ex.Message}");
            }
        }
    }
}

