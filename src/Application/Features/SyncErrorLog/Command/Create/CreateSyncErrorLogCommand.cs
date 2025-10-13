using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Create
{
    public class CreateSyncErrorLogCommand : IRequest<Result<SyncErrorLogDto>>
    {
        public CreateSyncErrorLogDto? SyncErrorLog { get; set; }
    }
}

