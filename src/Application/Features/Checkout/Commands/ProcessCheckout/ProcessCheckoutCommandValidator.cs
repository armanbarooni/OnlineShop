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

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping cost cannot be negative");

            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 1)
                .WithMessage("Tax rate must be between 0 and 1 (0-100%)");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount amount cannot be negative");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}

