using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Features.SavedCart.Command.Update
{
    public class UpdateSavedCartCommand : IRequest<Result<SavedCartDto>>
    {
        public UpdateSavedCartDto SavedCart { get; set; } = null!;
    }
}

