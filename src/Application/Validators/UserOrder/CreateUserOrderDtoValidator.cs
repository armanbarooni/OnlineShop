using FluentValidation;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Validators.UserOrder
{
    public class CreateUserOrderDtoValidator : AbstractValidator<CreateUserOrderDto>
    {
        public CreateUserOrderDtoValidator()
        {
            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .WithMessage("Order number is required")
                .MaximumLength(50)
                .WithMessage("Order number cannot exceed 50 characters");

            RuleFor(x => x.SubTotal)
                .GreaterThan(0)
                .WithMessage("Sub total must be greater than 0");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tax amount cannot be negative");

            RuleFor(x => x.ShippingAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping amount cannot be negative");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount amount cannot be negative");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0)
                .WithMessage("Total amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .MaximumLength(10)
                .WithMessage("Currency cannot exceed 10 characters");
        }
    }
}
