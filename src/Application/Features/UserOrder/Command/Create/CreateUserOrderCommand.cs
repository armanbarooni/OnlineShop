using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Command.Create
{
    public class CreateUserOrderCommand : IRequest<Result<UserOrderDto>>
    {
        public CreateUserOrderDto? UserOrder { get; set; }
        public Guid UserId { get; set; }
    }
}
