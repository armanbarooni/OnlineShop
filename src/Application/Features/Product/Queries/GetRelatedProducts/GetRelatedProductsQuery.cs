using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.GetRelatedProducts
{
    public class GetRelatedProductsQuery : IRequest<Result<List<ProductDto>>>
    {
        public Guid ProductId { get; set; }
        public string RelationType { get; set; } = "Similar";
        public int Limit { get; set; } = 10;
    }
}

