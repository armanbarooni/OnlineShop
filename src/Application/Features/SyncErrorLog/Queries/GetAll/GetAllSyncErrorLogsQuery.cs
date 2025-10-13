using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Queries.GetAll
{
    public class GetAllSyncErrorLogsQuery : IRequest<Result<List<SyncErrorLogDto>>>
    {
    }
}

