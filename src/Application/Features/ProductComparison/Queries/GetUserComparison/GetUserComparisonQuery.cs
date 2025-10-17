using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductComparison;

namespace OnlineShop.Application.Features.ProductComparison.Queries.GetUserComparison
{
    public class GetUserComparisonQuery : IRequest<Result<ProductComparisonDto?>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}

