using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Command.Create
{
    public class CreateProductInventoryCommand : IRequest<Result<ProductInventoryDto>>
    {
        public CreateProductInventoryDto? ProductInventory { get; set; }
    }
}
