using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Command.Create
{
    public record CreateProductCommand(CreateProductDto Product) : IRequest<Result<ProductDto>>;
}