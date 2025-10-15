using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Wishlist.Command.Delete
{
    public class DeleteWishlistCommand : IRequest<Result<bool>>
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
    }
}

