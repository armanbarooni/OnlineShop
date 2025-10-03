using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Command.Update
{
    public record UpdateProductCommand : IRequest<Result<ProductDto>>
    {
        public UpdateProductDto? Product { get; set; }
    }
}