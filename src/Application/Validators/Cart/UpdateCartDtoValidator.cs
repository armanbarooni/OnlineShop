using FluentValidation;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Validators.Cart
{
    public class UpdateCartDtoValidator : AbstractValidator<UpdateCartDto>
    {
        public UpdateCartDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Cart ID is required");

            RuleFor(x => x.CartName)
                .NotEmpty()
                .WithMessage("Cart name is required")
                .MaximumLength(100)
                .WithMessage("Cart name cannot exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.Now)
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }
    }
}

