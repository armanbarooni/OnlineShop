using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.MahakQueue.Command.Delete
{
    public class DeleteMahakQueueCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}


