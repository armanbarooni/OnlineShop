using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductComparison;

namespace OnlineShop.Application.Features.ProductComparison.Queries.CompareProducts
{
    public class CompareProductsQuery : IRequest<Result<ComparisonResultDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}

