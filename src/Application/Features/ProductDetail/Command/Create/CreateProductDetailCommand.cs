using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;

namespace OnlineShop.Application.Features.ProductDetail.Command.Create
{
    public class CreateProductDetailCommand : IRequest<Result<ProductDetailDto>>
    {
        public CreateProductDetailDto? ProductDetail { get; set; }
    }
}

