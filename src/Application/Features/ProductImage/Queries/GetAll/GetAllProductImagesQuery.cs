using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Features.ProductImage.Queries.GetAll
{
    public class GetAllProductImagesQuery : IRequest<Result<IEnumerable<ProductImageDto>>>
    {
    }
}
