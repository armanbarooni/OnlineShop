using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Create
{
    public class CreateSyncErrorLogCommandHandler(
        ISyncErrorLogRepository repository,
        IMapper mapper) : IRequestHandler<CreateSyncErrorLogCommand, Result<SyncErrorLogDto>>
    {
        public async Task<Result<SyncErrorLogDto>> Handle(CreateSyncErrorLogCommand request, CancellationToken cancellationToken)
        {
            if (request.SyncErrorLog == null)
                return Result<SyncErrorLogDto>.Failure("داده‌های لاگ خطا نباید خالی باشد");

            try
            {
                var syncErrorLog = Domain.Entities.SyncErrorLog.Create(
                    request.SyncErrorLog.ErrorType,
                    request.SyncErrorLog.EntityType,
                    request.SyncErrorLog.EntityId,
                    request.SyncErrorLog.ErrorCode,
                    request.SyncErrorLog.ErrorMessage,
                    request.SyncErrorLog.ErrorSeverity,
                    request.SyncErrorLog.RequestData,
                    request.SyncErrorLog.ResponseData
                );

                await repository.AddAsync(syncErrorLog, cancellationToken);
                return Result<SyncErrorLogDto>.Success(mapper.Map<SyncErrorLogDto>(syncErrorLog));
            }
            catch (Exception ex)
            {
                return Result<SyncErrorLogDto>.Failure($"خطا در ایجاد لاگ خطا: {ex.Message}");
            }
        }
    }
}

