using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;

namespace OnlineShop.Application.Features.ProductDetail.Queries.GetById
{
    public class GetProductDetailByIdQuery : IRequest<Result<ProductDetailDto>>
    {
        public Guid Id { get; set; }
    }
}

