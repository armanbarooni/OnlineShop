using FluentValidation;
using OnlineShop.Application.DTOs.Auth;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Validators.Auth
{
    public class RegisterWithPhoneDtoValidator : AbstractValidator<RegisterWithPhoneDto>
    {
        public RegisterWithPhoneDtoValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Must(BeValidIranianPhoneNumber).WithMessage("شماره تلفن معتبر نیست (فرمت: 09xxxxxxxxx)");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد تایید الزامی است")
                .Length(4, 8).WithMessage("کد تایید باید بین 4 تا 8 رقم باشد")
                .Must(BeNumeric).WithMessage("کد تایید باید عددی باشد");

            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("نام نباید بیشتر از 100 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("نام خانوادگی نباید بیشتر از 100 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Password));
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

