using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Queries.GetById
{
    public class GetProductReviewByIdQuery : IRequest<Result<ProductReviewDto>>
    {
        public Guid Id { get; set; }
    }
}

