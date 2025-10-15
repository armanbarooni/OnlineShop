using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Queries.GetById
{
    public record GetProductVariantByIdQuery : IRequest<Result<ProductVariantDto>>
    {
        public Guid Id { get; set; }
    }
}
