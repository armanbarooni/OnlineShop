using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Queries.GetAll
{
    public class GetAllWishlistsQuery : IRequest<Result<List<WishlistDto>>>
    {
    }
}

