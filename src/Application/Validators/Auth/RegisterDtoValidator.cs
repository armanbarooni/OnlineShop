using FluentValidation;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Validators.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است")
                .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است")
                .MaximumLength(256).WithMessage("ایمیل نباید بیشتر از ۲۵۶ کاراکتر باشد");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"\d").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد")
                .Matches(@"[\W_]").WithMessage("رمز عبور باید حداقل یک کاراکتر خاص داشته باشد");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن باید یکسان باشند");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(100).WithMessage("نام نباید بیشتر از ۱۰۰ کاراکتر باشد");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("نام خانوادگی الزامی است")
                .MaximumLength(100).WithMessage("نام خانوادگی نباید بیشتر از ۱۰۰ کاراکتر باشد");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^09\d{9}$").WithMessage("فرمت شماره موبایل نامعتبر است")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
