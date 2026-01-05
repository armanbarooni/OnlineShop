using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;

namespace OnlineShop.Application.Features.User.Queries.GetOrder
{
    public class GetUserOrderQuery : IRequest<Result<OrderDetailDto>>
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
    }
}
