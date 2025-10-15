using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetByUserId
{
    public class GetUserOrdersByUserIdQuery : IRequest<Result<IEnumerable<UserOrderDto>>>
    {
        public Guid UserId { get; set; }
    }
}

