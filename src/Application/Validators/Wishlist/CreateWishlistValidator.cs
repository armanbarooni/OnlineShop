using FluentValidation;
using OnlineShop.Application.DTOs.Wishlist;

namespace OnlineShop.Application.Validators.Wishlist
{
    public class CreateWishlistValidator : AbstractValidator<CreateWishlistDto>
    {
        public CreateWishlistValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithMessage("Notes cannot exceed 500 characters");
        }
    }
}
