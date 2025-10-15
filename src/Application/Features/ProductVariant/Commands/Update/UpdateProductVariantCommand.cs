using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Commands.Update
{
    public record UpdateProductVariantCommand : IRequest<Result<ProductVariantDto>>
    {
        public UpdateProductVariantDto ProductVariant { get; set; } = null!;
    }
}
