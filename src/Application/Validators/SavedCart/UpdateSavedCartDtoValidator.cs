using FluentValidation;
using OnlineShop.Application.DTOs.SavedCart;

namespace OnlineShop.Application.Validators.SavedCart
{
    public class UpdateSavedCartDtoValidator : AbstractValidator<UpdateSavedCartDto>
    {
        public UpdateSavedCartDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Saved Cart ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

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

