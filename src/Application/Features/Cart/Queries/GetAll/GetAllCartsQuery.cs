using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Queries.GetAll
{
    public class GetAllCartsQuery : IRequest<Result<List<CartDto>>>
    {
    }
}

