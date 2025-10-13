using FluentValidation;
using OnlineShop.Application.DTOs.ProductImage;

namespace OnlineShop.Application.Validators.ProductImage
{
    public class UpdateProductImageDtoValidator : AbstractValidator<UpdateProductImageDto>
    {
        public UpdateProductImageDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Image ID is required");

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

            RuleFor(x => x.Title)
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order cannot be negative");

            RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .WithMessage("File size must be greater than 0")
                .LessThanOrEqualTo(10485760) // 10MB
                .WithMessage("File size cannot exceed 10MB");

            RuleFor(x => x.MimeType)
                .MaximumLength(100)
                .WithMessage("MIME type cannot exceed 100 characters");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}

