using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Commands.AddToCart
{
    public class AddToCartCommand : IRequest<Result<CartDto>>
    {
        public Guid UserId { get; set; }
        public AddToCartDto Item { get; set; } = null!;
    }
}
