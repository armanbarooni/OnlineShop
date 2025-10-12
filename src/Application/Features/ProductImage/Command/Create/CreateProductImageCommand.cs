using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Command.Create
{
    public class CreateProductImageCommand : IRequest<Result<ProductImageDto>>
    {
        public CreateProductImageDto ProductImage { get; set; } = null!;
    }
}
