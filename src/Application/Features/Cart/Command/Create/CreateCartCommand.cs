using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Command.Create
{
    public class CreateCartCommand : IRequest<Result<CartDto>>
    {
        public CreateCartDto? Cart { get; set; }
        public Guid UserId { get; set; }
    }
}

