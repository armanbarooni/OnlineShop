using FluentValidation;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Validators.ProductReview
{
    public class ApproveProductReviewDtoValidator : AbstractValidator<ApproveProductReviewDto>
    {
        public ApproveProductReviewDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Review ID is required");

            RuleFor(x => x.AdminNotes)
                .MaximumLength(1000)
                .WithMessage("Admin notes cannot exceed 1000 characters");
        }
    }
}

