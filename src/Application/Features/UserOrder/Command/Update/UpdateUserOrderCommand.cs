using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Features.UserOrder.Command.Update
{
    public class UpdateUserOrderCommand : IRequest<Result<UserOrderDto>>
    {
        public UpdateUserOrderDto UserOrder { get; set; } = null!;
    }
}

