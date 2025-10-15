using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Queries.GetById
{
    public class GetMahakSyncLogByIdQuery : IRequest<Result<MahakSyncLogDto>>
    {
        public Guid Id { get; set; }
    }
}


