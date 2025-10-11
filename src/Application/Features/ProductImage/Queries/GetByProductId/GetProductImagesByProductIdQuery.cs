using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetByProductId
{
    public class GetProductImagesByProductIdQuery : IRequest<Result<IEnumerable<ProductImageDto>>>
    {
        public Guid ProductId { get; set; }
    }
}
