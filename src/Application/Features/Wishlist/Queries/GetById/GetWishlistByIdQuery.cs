using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Features.Wishlist.Queries.GetById
{
    public class GetWishlistByIdQuery : IRequest<Result<WishlistDto>>
    {
        public Guid Id { get; set; }
    }
}


