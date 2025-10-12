using FluentValidation;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Validators.UserPayment
{
    public class CreateUserPaymentDtoValidator : AbstractValidator<CreateUserPaymentDto>
    {
        public CreateUserPaymentDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .WithMessage("Payment method is required")
                .MaximumLength(50)
                .WithMessage("Payment method cannot exceed 50 characters");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required")
                .Length(3)
                .WithMessage("Currency must be exactly 3 characters");
        }
    }
}
