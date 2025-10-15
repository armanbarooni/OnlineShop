using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Queries.GetByProductId
{
    public record GetProductVariantsByProductIdQuery : IRequest<Result<List<ProductVariantDto>>>
    {
        public Guid ProductId { get; set; }
    }
}

