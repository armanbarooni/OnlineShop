using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Command.Create
{
    public class CreateWishlistCommand : IRequest<Result<WishlistDto>>
    {
        public CreateWishlistDto? Wishlist { get; set; }
        public Guid UserId { get; set; }
    }
}

