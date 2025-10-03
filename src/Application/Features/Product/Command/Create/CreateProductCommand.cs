using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Product;

namespace OnlineShop.Application.Features.Product.Command.Create
{
    public class CreateProductCommand: IRequest<Result<ProductDto>>
    {
        public CreateProductDto? Product { get; set; }
    }
}