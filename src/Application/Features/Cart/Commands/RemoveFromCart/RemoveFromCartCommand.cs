using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Commands.RemoveFromCart
{
    public class RemoveFromCartCommand : IRequest<Result<CartDto>>
    {
        public Guid UserId { get; set; }
        public Guid CartItemId { get; set; }
    }
}
