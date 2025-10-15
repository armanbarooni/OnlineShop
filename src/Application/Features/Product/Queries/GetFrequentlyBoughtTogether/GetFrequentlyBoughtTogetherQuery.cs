using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Queries.GetFrequentlyBoughtTogether
{
    public class GetFrequentlyBoughtTogetherQuery : IRequest<Result<List<ProductDto>>>
    {
        public Guid ProductId { get; set; }
        public int Limit { get; set; } = 10;
    }
}

