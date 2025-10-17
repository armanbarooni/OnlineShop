using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductComparison.Commands.RemoveFromComparison
{
    public class RemoveFromComparisonCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
    }
}

