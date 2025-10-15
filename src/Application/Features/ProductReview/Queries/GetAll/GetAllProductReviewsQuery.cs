using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Queries.GetAll
{
    public class GetAllProductReviewsQuery : IRequest<Result<List<ProductReviewDto>>>
    {
    }
}


