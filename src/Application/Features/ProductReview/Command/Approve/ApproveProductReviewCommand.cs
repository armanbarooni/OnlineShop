using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Command.Approve
{
    public class ApproveProductReviewCommand : IRequest<Result<ProductReviewDto>>
    {
        public Guid Id { get; set; }
        public string? AdminNotes { get; set; }
    }
}

