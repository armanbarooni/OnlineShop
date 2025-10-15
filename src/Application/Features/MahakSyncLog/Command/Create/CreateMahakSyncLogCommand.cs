using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Command.Create
{
    public class CreateMahakSyncLogCommand : IRequest<Result<MahakSyncLogDto>>
    {
        public CreateMahakSyncLogDto? MahakSyncLog { get; set; }
    }
}


