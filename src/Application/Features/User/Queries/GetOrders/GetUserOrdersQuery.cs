using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;

namespace OnlineShop.Application.Features.User.Queries.GetOrders
{
    public class GetUserOrdersQuery : IRequest<Result<List<OrderDto>>>
    {
        public Guid UserId { get; set; }
    }
}
