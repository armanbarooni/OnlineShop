using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public class UpdateProductCategoryCommand : IRequest<Result<ProductCategoryDto>>
    {
        public Guid Id { get; set; }
        public UpdateProductCategoryDto? Dto { get; set; }
    }
}
