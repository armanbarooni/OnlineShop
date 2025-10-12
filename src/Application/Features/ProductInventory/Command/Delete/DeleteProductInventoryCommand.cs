using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductInventory.Command.Delete
{
    public class DeleteProductInventoryCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}
