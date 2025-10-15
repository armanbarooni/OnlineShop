using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetById
{
    public class GetProductImageByIdQuery : IRequest<Result<ProductImageDto>>
    {
        public Guid Id { get; set; }
    }
}

