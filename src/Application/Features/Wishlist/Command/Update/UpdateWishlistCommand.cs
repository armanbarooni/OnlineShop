using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Command.Update
{
    public class UpdateWishlistCommand : IRequest<Result<WishlistDto>>
    {
        public UpdateWishlistDto? Wishlist { get; set; }
    }
}


