using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.SyncErrorLog;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.SyncErrorLog.Queries.GetAll
{
    public class GetAllSyncErrorLogsQueryHandler(
        ISyncErrorLogRepository repository,
        IMapper mapper) : IRequestHandler<GetAllSyncErrorLogsQuery, Result<List<SyncErrorLogDto>>>
    {
        public async Task<Result<List<SyncErrorLogDto>>> Handle(GetAllSyncErrorLogsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var syncErrorLogs = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<SyncErrorLogDto>>(syncErrorLogs);
                return Result<List<SyncErrorLogDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<SyncErrorLogDto>>.Failure($"خطا در دریافت لیست لاگ‌های خطا: {ex.Message}");
            }
        }
    }
}



