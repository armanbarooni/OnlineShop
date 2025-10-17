using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductComparison;

namespace OnlineShop.Application.Features.ProductComparison.Commands.AddToComparison
{
    public class AddToComparisonCommand : IRequest<Result<ProductComparisonDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
    }
}

