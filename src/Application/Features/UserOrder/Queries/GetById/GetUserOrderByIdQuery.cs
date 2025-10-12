using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Queries.GetById
{
    public class GetUserOrderByIdQuery : IRequest<Result<UserOrderDto>>
    {
        public Guid Id { get; set; }
    }
}
