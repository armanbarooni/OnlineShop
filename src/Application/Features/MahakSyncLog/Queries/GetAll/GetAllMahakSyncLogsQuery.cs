using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Queries.GetAll
{
    public class GetAllMahakSyncLogsQuery : IRequest<Result<List<MahakSyncLogDto>>>
    {
    }
}

