using FluentValidation;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Validators.Auth
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Matches(@"^09\d{9}$").WithMessage("فرمت شماره تلفن نامعتبر است");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("کد تایید الزامی است");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد");
        }
    }
}
