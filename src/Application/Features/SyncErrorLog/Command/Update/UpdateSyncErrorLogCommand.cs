using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Update
{
    public class UpdateSyncErrorLogCommand : IRequest<Result<SyncErrorLogDto>>
    {
        public UpdateSyncErrorLogDto? SyncErrorLog { get; set; }
    }
}

