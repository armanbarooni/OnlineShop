using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Queries.GetByUserId
{
    public class GetWishlistByUserIdQuery : IRequest<Result<IEnumerable<WishlistDto>>>
    {
        public Guid UserId { get; set; }
    }
}

