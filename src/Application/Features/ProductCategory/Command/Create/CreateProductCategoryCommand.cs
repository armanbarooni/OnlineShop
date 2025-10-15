using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Create
{
    public class CreateProductCategoryCommand : IRequest<Result<ProductCategoryDto>>
    {
        public CreateProductCategoryDto? Dto { get; set; }
    }
}

