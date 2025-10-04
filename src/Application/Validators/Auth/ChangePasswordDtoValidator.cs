using FluentValidation;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Validators.Auth
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور جدید باید حداقل ۸ کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور جدید باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور جدید باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"\d").WithMessage("رمز عبور جدید باید حداقل یک عدد داشته باشد")
                .Matches(@"[\W_]").WithMessage("رمز عبور جدید باید حداقل یک کاراکتر خاص داشته باشد")
                .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید نباید با رمز عبور فعلی یکسان باشد");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است")
                .Equal(x => x.NewPassword).WithMessage("رمز عبور جدید و تکرار آن باید یکسان باشند");
        }
    }
}
