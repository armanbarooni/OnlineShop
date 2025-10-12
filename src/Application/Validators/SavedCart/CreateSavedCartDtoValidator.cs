using FluentValidation;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Validators.SavedCart
{
    public class CreateSavedCartDtoValidator : AbstractValidator<CreateSavedCartDto>
    {
        public CreateSavedCartDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("Cart ID is required");

            RuleFor(x => x.SavedCartName)
                .NotEmpty()
                .WithMessage("Saved cart name is required")
                .MaximumLength(100)
                .WithMessage("Saved cart name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters");
        }
    }
}
