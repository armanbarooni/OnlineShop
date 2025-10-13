using FluentValidation;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Features.Auth.Commands.RegisterWithPhone
{
    public class RegisterWithPhoneCommandValidator : AbstractValidator<RegisterWithPhoneCommand>
    {
        public RegisterWithPhoneCommandValidator()
        {
            RuleFor(x => x.Request.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Must(BeValidIranianPhoneNumber).WithMessage("شماره تلفن معتبر نیست (فرمت: 09xxxxxxxxx)");

            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("کد تایید الزامی است")
                .Length(4, 8).WithMessage("کد تایید باید بین 4 تا 8 رقم باشد")
                .Must(BeNumeric).WithMessage("کد تایید باید عددی باشد");

            RuleFor(x => x.Request.FirstName)
                .MaximumLength(100).WithMessage("نام نباید بیشتر از 100 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Request.FirstName));

            RuleFor(x => x.Request.LastName)
                .MaximumLength(100).WithMessage("نام خانوادگی نباید بیشتر از 100 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Request.LastName));

            RuleFor(x => x.Request.Password)
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Request.Password));
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

