using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductComparison.Commands.ClearComparison
{
    public class ClearComparisonCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = string.Empty;
    }
}

