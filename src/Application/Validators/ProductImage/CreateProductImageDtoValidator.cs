using FluentValidation;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Validators.ProductImage
{
    public class CreateProductImageDtoValidator : AbstractValidator<CreateProductImageDto>
    {
        public CreateProductImageDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Image URL is required")
                .MaximumLength(500)
                .WithMessage("Image URL cannot exceed 500 characters")
                .Must(BeValidUrl)
                .WithMessage("Image URL must be a valid URL");

            RuleFor(x => x.AltText)
                .MaximumLength(200)
                .WithMessage("Alt text cannot exceed 200 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order cannot be negative");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
