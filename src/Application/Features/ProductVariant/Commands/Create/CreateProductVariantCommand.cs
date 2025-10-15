using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Commands.Create
{
    public record CreateProductVariantCommand : IRequest<Result<ProductVariantDto>>
    {
        public CreateProductVariantDto ProductVariant { get; set; } = null!;
    }
}
