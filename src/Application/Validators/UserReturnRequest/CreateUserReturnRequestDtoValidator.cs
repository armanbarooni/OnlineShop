using FluentValidation;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Validators.UserReturnRequest
{
    public class CreateUserReturnRequestDtoValidator : AbstractValidator<CreateUserReturnRequestDto>
    {
        public CreateUserReturnRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithMessage("Order ID is required");

            RuleFor(x => x.OrderItemId)
                .NotEmpty()
                .WithMessage("Order item ID is required");

            RuleFor(x => x.ReturnReason)
                .NotEmpty()
                .WithMessage("Return reason is required")
                .MaximumLength(500)
                .WithMessage("Return reason cannot exceed 500 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.RefundAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Refund amount cannot be negative");

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Description cannot exceed 1000 characters");
        }
    }
}
