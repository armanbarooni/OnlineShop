using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetRecentOrders
{
    public class GetRecentUserOrdersQuery : IRequest<Result<IEnumerable<UserOrderDto>>>
    {
        public Guid UserId { get; set; }
        public int Limit { get; set; } = 5;
    }
}

