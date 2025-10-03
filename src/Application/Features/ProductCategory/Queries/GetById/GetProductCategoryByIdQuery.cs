using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Queries.GetById
{
    public class GetProductCategoryByIdQuery: IRequest<Result<ProductCategoryDto>>
    {
        public Guid Id { get; set; }
    }
}
