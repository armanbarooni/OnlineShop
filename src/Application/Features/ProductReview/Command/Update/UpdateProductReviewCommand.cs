using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Command.Update
{
    public class UpdateProductReviewCommand : IRequest<Result<ProductReviewDto>>
    {
        public UpdateProductReviewDto? ProductReview { get; set; }
    }
}

