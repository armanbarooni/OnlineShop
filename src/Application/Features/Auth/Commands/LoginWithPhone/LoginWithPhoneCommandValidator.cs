using FluentValidation;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Features.Auth.Commands.LoginWithPhone
{
    public class LoginWithPhoneCommandValidator : AbstractValidator<LoginWithPhoneCommand>
    {
        public LoginWithPhoneCommandValidator()
        {
            RuleFor(x => x.Request.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Must(BeValidIranianPhoneNumber).WithMessage("شماره تلفن معتبر نیست (فرمت: 09xxxxxxxxx)");

            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("کد تایید الزامی است")
                .Length(4, 8).WithMessage("کد تایید باید بین 4 تا 8 رقم باشد")
                .Must(BeNumeric).WithMessage("کد تایید باید عددی باشد");
        }

        private bool BeValidIranianPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var pattern = @"^09\d{9}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool BeNumeric(string code)
        {
            return !string.IsNullOrWhiteSpace(code) && code.All(char.IsDigit);
        }
    }
}

