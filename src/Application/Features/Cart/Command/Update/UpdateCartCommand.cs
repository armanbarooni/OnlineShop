using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Command.Update
{
    public class UpdateCartCommand : IRequest<Result<CartDto>>
    {
        public UpdateCartDto? Cart { get; set; }
    }
}

