using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakSyncLog;

namespace OnlineShop.Application.Features.MahakSyncLog.Command.Update
{
    public class UpdateMahakSyncLogCommand : IRequest<Result<MahakSyncLogDto>>
    {
        public UpdateMahakSyncLogDto? MahakSyncLog { get; set; }
    }
}


