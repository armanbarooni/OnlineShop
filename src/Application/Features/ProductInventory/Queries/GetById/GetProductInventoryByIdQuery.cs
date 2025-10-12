using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Queries.GetById
{
    public class GetProductInventoryByIdQuery : IRequest<Result<ProductInventoryDto>>
    {
        public Guid Id { get; set; }
    }
}
