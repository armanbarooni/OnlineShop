using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Queries.GetById
{
    public class GetMahakSyncLogByIdQueryHandler(
        IMahakSyncLogRepository repository,
        IMapper mapper) : IRequestHandler<GetMahakSyncLogByIdQuery, Result<MahakSyncLogDto>>
    {
        public async Task<Result<MahakSyncLogDto>> Handle(GetMahakSyncLogByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mahakSyncLog = await repository.GetByIdAsync(request.Id, cancellationToken);
                
                if (mahakSyncLog == null)
                    return Result<MahakSyncLogDto>.Failure("لاگ سینک محک یافت نشد");

                var dto = mapper.Map<MahakSyncLogDto>(mahakSyncLog);
                return Result<MahakSyncLogDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<MahakSyncLogDto>.Failure($"خطا در دریافت لاگ سینک محک: {ex.Message}");
            }
        }
    }
}

