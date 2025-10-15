using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Command.Create
{
    public class CreateSavedCartCommand : IRequest<Result<SavedCartDto>>
    {
        public CreateSavedCartDto SavedCart { get; set; } = null!;
    }
}

