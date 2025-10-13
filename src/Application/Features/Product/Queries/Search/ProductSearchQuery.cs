using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Common;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.Search
{
    public class ProductSearchQuery : IRequest<Result<PagedResultDto<ProductDto>>>
    {
        public ProductSearchCriteriaDto? Criteria { get; set; }
    }
}

