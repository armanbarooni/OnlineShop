using FluentValidation;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Validators.UserPayment
{
    public class UpdateUserPaymentDtoValidator : AbstractValidator<UpdateUserPaymentDto>
    {
        public UpdateUserPaymentDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Payment ID is required");

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
                .MaximumLength(10)
                .WithMessage("Currency cannot exceed 10 characters")
                .Must(BeValidCurrency)
                .WithMessage("Invalid currency. Valid currencies: IRR, USD, EUR");
        }

        private static bool BeValidCurrency(string currency)
        {
            var validCurrencies = new[] { "IRR", "USD", "EUR", "GBP" };
            return validCurrencies.Contains(currency.ToUpper());
        }
    }
}

