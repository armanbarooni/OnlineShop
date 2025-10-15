using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.DTOs.MahakSyncLog;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.MahakSyncLog.Queries.GetAll
{
    public class GetAllMahakSyncLogsQueryHandler(
        IMahakSyncLogRepository repository,
        IMapper mapper) : IRequestHandler<GetAllMahakSyncLogsQuery, Result<List<MahakSyncLogDto>>>
    {
        public async Task<Result<List<MahakSyncLogDto>>> Handle(GetAllMahakSyncLogsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakSyncLogs = await repository.GetAllAsync(cancellationToken);
                var dtoList = mapper.Map<List<MahakSyncLogDto>>(mahakSyncLogs);
                return Result<List<MahakSyncLogDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<MahakSyncLogDto>>.Failure($"خطا در دریافت لیست لاگ‌های سینک محک: {ex.Message}");
            }
        }
    }
}



