using FluentValidation;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Validators.UserOrder
{
    public class CreateUserOrderDtoValidator : AbstractValidator<CreateUserOrderDto>
    {
        public CreateUserOrderDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .WithMessage("Order number is required")
                .MaximumLength(50)
                .WithMessage("Order number cannot exceed 50 characters");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0)
                .WithMessage("Total amount must be greater than 0");

            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Shipping cost cannot be negative");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Discount amount cannot be negative");

            RuleFor(x => x.FinalAmount)
                .GreaterThan(0)
                .WithMessage("Final amount must be greater than 0");

            RuleFor(x => x.ShippingAddress)
                .NotEmpty()
                .WithMessage("Shipping address is required")
                .MaximumLength(500)
                .WithMessage("Shipping address cannot exceed 500 characters");
        }
    }
}
