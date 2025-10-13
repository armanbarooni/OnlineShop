using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.SyncErrorLog.Command.Delete
{
    public class DeleteSyncErrorLogCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

