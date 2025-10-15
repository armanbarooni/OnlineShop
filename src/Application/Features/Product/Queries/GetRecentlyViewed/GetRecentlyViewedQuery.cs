using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.GetRecentlyViewed
{
    public class GetRecentlyViewedQuery : IRequest<Result<List<ProductDto>>>
    {
        public string UserId { get; set; } = string.Empty;
        public int Limit { get; set; } = 20;
    }
}

