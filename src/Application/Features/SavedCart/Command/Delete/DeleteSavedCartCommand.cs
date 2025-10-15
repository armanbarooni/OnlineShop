using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.SavedCart.Command.Delete
{
    public class DeleteSavedCartCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

