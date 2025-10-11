using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Command.Update
{
    public class UpdateProductImageCommand : IRequest<Result<ProductImageDto>>
    {
        public UpdateProductImageDto? ProductImage { get; set; }
    }
}
