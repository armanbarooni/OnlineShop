using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductDetail;

namespace OnlineShop.Application.Features.ProductDetail.Queries.GetAll
{
    public class GetAllProductDetailsQuery : IRequest<Result<IEnumerable<ProductDetailDto>>>
    {
    }
}

