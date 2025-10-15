using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Features.ProductReview.Command.Reject
{
    public class RejectProductReviewCommand : IRequest<Result<ProductReviewDto>>
    {
        public Guid Id { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
    }
}

