using FluentValidation;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
    {
        public SendOtpCommandValidator()
        {
            RuleFor(x => x.Request.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Must(BeValidIranianPhoneNumber).WithMessage("شماره تلفن معتبر نیست (فرمت: 09xxxxxxxxx)");

            RuleFor(x => x.Request.Purpose)
                .NotEmpty().WithMessage("هدف ارسال OTP الزامی است")
                .Must(BeValidPurpose).WithMessage("هدف نامعتبر است. مقادیر مجاز: Login, Registration, PasswordReset");
        }

        private bool BeValidIranianPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

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


