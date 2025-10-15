using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.MahakQueue;

namespace OnlineShop.Application.Features.MahakQueue.Queries.GetAll
{
    public class GetAllMahakQueuesQuery : IRequest<Result<List<MahakQueueDto>>>
    {
    }
}


