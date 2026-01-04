using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Order;

namespace OnlineShop.Application.Features.Order.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<Result<OrderDto>>
    {
        public Guid UserId { get; set; }
        public CreateOrderDto Request { get; set; } = null!;
    }
}
