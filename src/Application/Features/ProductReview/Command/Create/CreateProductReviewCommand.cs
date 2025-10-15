using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Command.Create
{
    public class CreateProductReviewCommand : IRequest<Result<ProductReviewDto>>
    {
        public CreateProductReviewDto? ProductReview { get; set; }
        public Guid UserId { get; set; }
    }
}

