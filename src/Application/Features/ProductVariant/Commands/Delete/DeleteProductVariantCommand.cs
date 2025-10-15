using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductVariant.Commands.Delete
{
    public record DeleteProductVariantCommand(Guid Id) : IRequest<Result<bool>>;
}
