using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetCategoryTree
{
    public class GetCategoryTreeQuery : IRequest<Result<List<ProductCategoryDto>>>
    {
    }
}

