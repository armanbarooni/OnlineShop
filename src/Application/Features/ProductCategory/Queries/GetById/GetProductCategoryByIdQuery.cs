using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public record GetProductCategoryByIdQuery(Guid Id)
        : IRequest<Result<ProductCategoryDto>>;
}
