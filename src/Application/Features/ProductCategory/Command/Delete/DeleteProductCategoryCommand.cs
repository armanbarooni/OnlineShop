using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductCategory.Command.Delete
{
    public class DeleteProductCategoryCommand : IRequest<Result<bool>>
    {
        public Guid Id{ get; set; }
    }
}
