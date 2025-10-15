using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Queries.GetByProductId
{
    public class GetProductReviewsByProductIdQuery : IRequest<Result<IEnumerable<ProductReviewDto>>>
    {
        public Guid ProductId { get; set; }
    }
}

