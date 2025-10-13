using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.ProductReview.Command.Delete
{
    public class DeleteProductReviewCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

