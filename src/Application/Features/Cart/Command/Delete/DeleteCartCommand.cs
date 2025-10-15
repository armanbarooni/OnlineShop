using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Cart.Command.Delete
{
    public class DeleteCartCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}


