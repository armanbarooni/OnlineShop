using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetAll
{
    public class GetAllUserOrdersQuery : IRequest<Result<IEnumerable<UserOrderDto>>>
    {
    }
}

