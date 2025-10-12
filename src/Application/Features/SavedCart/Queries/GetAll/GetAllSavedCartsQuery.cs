using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Queries.GetAll
{
    public class GetAllSavedCartsQuery : IRequest<Result<IEnumerable<SavedCartDto>>>
    {
    }
}
