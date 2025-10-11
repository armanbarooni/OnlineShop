using FluentValidation;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Validators.Cart
{
    public class CreateCartValidator : AbstractValidator<CreateCartDto>
    {
        public CreateCartValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage("Session ID is required")
                .MaximumLength(100)
                .WithMessage("Session ID cannot exceed 100 characters");

            RuleFor(x => x.CartName)
                .NotEmpty()
                .WithMessage("Cart name is required")
                .MaximumLength(100)
                .WithMessage("Cart name cannot exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow)
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }
    }
}
