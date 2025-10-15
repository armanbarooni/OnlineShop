using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Features.ProductVariant.Queries.GetAll
{
    public record GetAllProductVariantsQuery : IRequest<Result<List<ProductVariantDto>>>;
}

