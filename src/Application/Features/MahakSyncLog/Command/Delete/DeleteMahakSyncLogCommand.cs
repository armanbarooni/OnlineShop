using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.MahakSyncLog.Command.Delete
{
    public class DeleteMahakSyncLogCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}


