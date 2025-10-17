using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetSubCategories
{
    public class GetSubCategoriesQuery : IRequest<Result<List<ProductCategoryDto>>>
    {
        public Guid ParentId { get; set; }

        public GetSubCategoriesQuery(Guid parentId)
        {
            ParentId = parentId;
        }
    }
}

