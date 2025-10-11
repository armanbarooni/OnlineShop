using FluentValidation;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Validators.Cart
{
    public class CreateCartItemValidator : AbstractValidator<CreateCartItemDto>
    {
        public CreateCartItemValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("Cart ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");
        }
    }
}
