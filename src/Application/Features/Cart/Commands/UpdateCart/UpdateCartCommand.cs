using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Commands.UpdateCart
{
    public class UpdateCartCommand : IRequest<Result<CartDto>>
    {
        public Guid UserId { get; set; }
        public UpdateCartItemDto Item { get; set; } = null!;
    }
}
