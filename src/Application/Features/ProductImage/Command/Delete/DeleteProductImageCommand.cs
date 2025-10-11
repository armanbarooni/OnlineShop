using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductImage.Command.Delete
{
    public class DeleteProductImageCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}
