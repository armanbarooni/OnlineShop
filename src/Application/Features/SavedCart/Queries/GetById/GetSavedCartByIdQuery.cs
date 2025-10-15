using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Queries.GetById
{
    public class GetSavedCartByIdQuery : IRequest<Result<SavedCartDto>>
    {
        public Guid Id { get; set; }
    }
}

