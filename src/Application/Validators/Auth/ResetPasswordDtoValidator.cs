using FluentValidation;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Validators.Auth
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است")
                .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("توکن بازیابی الزامی است");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور جدید باید حداقل ۸ کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور جدید باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور جدید باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"\d").WithMessage("رمز عبور جدید باید حداقل یک عدد داشته باشد")
                .Matches(@"[\W_]").WithMessage("رمز عبور جدید باید حداقل یک کاراکتر خاص داشته باشد");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
                .Equal(x => x.NewPassword).WithMessage("رمز عبور و تکرار آن باید یکسان باشند");
        }
    }
}
