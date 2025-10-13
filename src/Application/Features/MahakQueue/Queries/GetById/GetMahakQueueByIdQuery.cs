using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Queries.GetById
{
    public class GetMahakQueueByIdQuery : IRequest<Result<MahakQueueDto>>
    {
        public Guid Id { get; set; }
    }
}

