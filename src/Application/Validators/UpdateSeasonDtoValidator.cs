using FluentValidation;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Validators
{
    public class UpdateSeasonDtoValidator : AbstractValidator<UpdateSeasonDto>
    {
        public UpdateSeasonDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("شناسه فصل الزامی است");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام فصل الزامی است")
                .MaximumLength(50).WithMessage("نام فصل نمی‌تواند بیش از 50 کاراکتر باشد");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد فصل الزامی است")
                .MaximumLength(10).WithMessage("کد فصل نمی‌تواند بیش از 10 کاراکتر باشد")
                .Matches("^[A-Z]+$").WithMessage("کد فصل باید فقط حروف بزرگ انگلیسی باشد");
        }
    }
}
