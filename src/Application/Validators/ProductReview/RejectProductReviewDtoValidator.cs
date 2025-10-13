using FluentValidation;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Validators.ProductReview
{
    public class RejectProductReviewDtoValidator : AbstractValidator<RejectProductReviewDto>
    {
        public RejectProductReviewDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Review ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required")
                .MaximumLength(500)
                .WithMessage("Rejection reason cannot exceed 500 characters");

            RuleFor(x => x.AdminNotes)
                .MaximumLength(1000)
                .WithMessage("Admin notes cannot exceed 1000 characters");
        }
    }
}

