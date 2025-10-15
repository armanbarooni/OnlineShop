using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Features.ProductInventory.Queries.GetAll
{
    public class GetAllProductInventoriesQuery : IRequest<Result<IEnumerable<ProductInventoryDto>>>
    {
    }
}

