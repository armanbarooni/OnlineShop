using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Queries.GetByUserId
{
    public class GetCartByUserIdQuery : IRequest<Result<CartDto?>>
    {
        public Guid UserId { get; set; }
    }
}

