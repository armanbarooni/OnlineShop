using FluentValidation;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandValidator : AbstractValidator<CheckoutRequestDto>
    {
        public ProcessCheckoutCommandValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty()
                .WithMessage("Cart ID is required");

            RuleFor(x => x.ShippingAddressId)
                .NotEmpty()
                .WithMessage("Shipping address is required");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}
