using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetAll
{
    public record GetAllProductCategoriesQuery : IRequest<Result<List<ProductCategoryDto>>>;
}

