using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Queries.GetById
{
    public class GetSyncErrorLogByIdQueryHandler(
        ISyncErrorLogRepository repository,
        IMapper mapper) : IRequestHandler<GetSyncErrorLogByIdQuery, Result<SyncErrorLogDto>>
    {
        public async Task<Result<SyncErrorLogDto>> Handle(GetSyncErrorLogByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var syncErrorLog = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (syncErrorLog == null)
                    return Result<SyncErrorLogDto>.Failure("لاگ خطا یافت نشد");

                var dto = mapper.Map<SyncErrorLogDto>(syncErrorLog);
                return Result<SyncErrorLogDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<SyncErrorLogDto>.Failure($"خطا در دریافت لاگ خطا: {ex.Message}");
            }
        }
    }
}

