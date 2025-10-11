using FluentValidation;
using OnlineShop.Application.DTOs.ProductDetail;

namespace OnlineShop.Application.Validators.ProductDetail
{
    public class UpdateProductDetailValidator : AbstractValidator<UpdateProductDetailDto>
    {
        public UpdateProductDetailValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("ID is required");

            RuleFor(x => x.Key)
                .NotEmpty()
                .WithMessage("Key is required")
                .MaximumLength(100)
                .WithMessage("Key cannot exceed 100 characters");

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage("Value is required")
                .MaximumLength(500)
                .WithMessage("Value cannot exceed 500 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be 0 or greater");
        }
    }
}
