using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Queries.GetByProductId
{
    public class GetProductInventoryByProductIdQuery : IRequest<Result<ProductInventoryDto>>
    {
        public Guid ProductId { get; set; }
    }
}

