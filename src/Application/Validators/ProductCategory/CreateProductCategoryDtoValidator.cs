using FluentValidation;
using OnlineShop.Application.DTOs.ProductCategory;

namespace OnlineShop.Application.Validators.ProductCategory
{
    public class CreateProductCategoryDtoValidator : AbstractValidator<CreateProductCategoryDto>
    {
        public CreateProductCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}
