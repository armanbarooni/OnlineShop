using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public record CreateProductCategoryCommand(CreateProductCategoryDto Dto)
        : IRequest<Result<ProductCategoryDto>>;
}
