using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.Cart.Command.RemoveItem
{
    public class RemoveCartItemCommand : IRequest<Result<bool>>
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
    }
}
