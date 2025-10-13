using FluentValidation;
using OnlineShop.Application.DTOs.Auth;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Validators.Auth
{
    public class SendOtpDtoValidator : AbstractValidator<SendOtpDto>
    {
        public SendOtpDtoValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Must(BeValidIranianPhoneNumber).WithMessage("شماره تلفن معتبر نیست (فرمت: 09xxxxxxxxx)");

            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("هدف ارسال OTP الزامی است")
                .Must(BeValidPurpose).WithMessage("هدف نامعتبر است. مقادیر مجاز: Login, Registration, PasswordReset");
        }

        private bool BeValidIranianPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Iranian mobile phone number pattern: 09xxxxxxxxx (11 digits)
            var pattern = @"^09\d{9}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool BeValidPurpose(string purpose)
        {
            var validPurposes = new[] { "Login", "Registration", "PasswordReset" };
            return validPurposes.Contains(purpose, StringComparer.OrdinalIgnoreCase);
        }
    }
}

