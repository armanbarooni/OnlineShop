using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderStatistics
{
    public class GetUserOrderStatisticsQuery : IRequest<Result<UserOrderStatisticsDto>>
    {
        public Guid UserId { get; set; }
    }
}

