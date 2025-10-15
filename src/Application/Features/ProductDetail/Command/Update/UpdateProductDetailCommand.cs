using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;

namespace OnlineShop.Application.Features.ProductDetail.Command.Update
{
    public class UpdateProductDetailCommand : IRequest<Result<ProductDetailDto>>
    {
        public UpdateProductDetailDto? ProductDetail { get; set; }
    }
}

