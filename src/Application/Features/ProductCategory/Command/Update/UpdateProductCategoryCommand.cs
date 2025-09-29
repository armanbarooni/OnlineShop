using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Features.ProductCategory.Command.Update
{
    public record UpdateProductCategoryCommand(Guid Id, UpdateProductCategoryDto Dto);
}
