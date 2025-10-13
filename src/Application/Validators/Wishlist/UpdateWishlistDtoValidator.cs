using FluentValidation;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Validators.Wishlist
{
    public class UpdateWishlistDtoValidator : AbstractValidator<UpdateWishlistDto>
    {
        public UpdateWishlistDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Wishlist ID is required");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");
        }
    }
}

