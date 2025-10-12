using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Queries.GetFavoriteCarts
{
    public class GetFavoriteSavedCartsQuery : IRequest<Result<IEnumerable<SavedCartDto>>>
    {
        public Guid UserId { get; set; }
    }
}
