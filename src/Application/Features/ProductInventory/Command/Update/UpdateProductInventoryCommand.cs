using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Command.Update
{
    public class UpdateProductInventoryCommand : IRequest<Result<ProductInventoryDto>>
    {
        public UpdateProductInventoryDto? ProductInventory { get; set; }
    }
}
