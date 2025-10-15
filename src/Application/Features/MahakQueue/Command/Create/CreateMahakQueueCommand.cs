using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Command.Create
{
    public class CreateMahakQueueCommand : IRequest<Result<MahakQueueDto>>
    {
        public CreateMahakQueueDto? MahakQueue { get; set; }
    }
}


