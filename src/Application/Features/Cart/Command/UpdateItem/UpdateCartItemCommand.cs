using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Command.UpdateItem
{
    public class UpdateCartItemCommand : IRequest<Result<CartItemDto>>
    {
        public UpdateCartItemDto? CartItem { get; set; }
    }
}
