using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetOrderItems
{
    public class GetUserOrderItemsQuery : IRequest<Result<IEnumerable<UserOrderItemDto>>>
    {
        public Guid OrderId { get; set; }
    }
}

