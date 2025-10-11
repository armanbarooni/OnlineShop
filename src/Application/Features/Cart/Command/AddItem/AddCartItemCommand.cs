using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Command.AddItem
{
    public class AddCartItemCommand : IRequest<Result<CartItemDto>>
    {
        public CreateCartItemDto? CartItem { get; set; }
    }
}
