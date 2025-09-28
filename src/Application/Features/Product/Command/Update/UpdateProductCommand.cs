using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Command.Update
{
    public record UpdateProductCommand(UpdateProductDto Product) : IRequest<Result<ProductDto>>;
}