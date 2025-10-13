using FluentValidation;
using OnlineShop.Application.DTOs.UserOrder;

namespace OnlineShop.Application.Validators.UserOrder
{
    public class UpdateUserOrderDtoValidator : AbstractValidator<UpdateUserOrderDto>
    {
        public UpdateUserOrderDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Order ID is required");

            RuleFor(x => x.OrderStatus)
                .NotEmpty()
                .WithMessage("Order status is required")
                .MaximumLength(50)
                .WithMessage("Order status cannot exceed 50 characters")
                .Must(BeValidOrderStatus)
                .WithMessage("Invalid order status. Valid statuses: Pending, Processing, Shipped, Delivered, Cancelled");

            RuleFor(x => x.SubTotal)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Sub total cannot be negative");

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

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters");

            RuleFor(x => x.TrackingNumber)
                .MaximumLength(100)
                .WithMessage("Tracking number cannot exceed 100 characters");
        }

        private static bool BeValidOrderStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Returned" };
            return validStatuses.Contains(status);
        }
    }
}

