using FluentValidation;
using OnlineShop.Application.DTOs.ProductReview;

namespace OnlineShop.Application.Validators.ProductReview
{
    public class UpdateProductReviewDtoValidator : AbstractValidator<UpdateProductReviewDto>
    {
        public UpdateProductReviewDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Review ID is required");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Review title is required")
                .MaximumLength(200)
                .WithMessage("Review title cannot exceed 200 characters");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .WithMessage("Comment is required")
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");
        }
    }
}

