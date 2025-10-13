using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SyncErrorLog;

namespace OnlineShop.Application.Features.SyncErrorLog.Queries.GetById
{
    public class GetSyncErrorLogByIdQuery : IRequest<Result<SyncErrorLogDto>>
    {
        public Guid Id { get; set; }
    }
}

