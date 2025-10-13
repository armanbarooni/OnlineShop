using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Queries.GetById
{
    public class GetCartByIdQuery : IRequest<Result<CartDto>>
    {
        public Guid Id { get; set; }
    }
}

