using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Command.Update
{
    public class UpdateMahakQueueCommand : IRequest<Result<MahakQueueDto>>
    {
        public UpdateMahakQueueDto? MahakQueue { get; set; }
    }
}


