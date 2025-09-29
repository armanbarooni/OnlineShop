using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public record DeleteProductCategoryCommand(Guid Id) : IRequest<Result<bool>>;
}
