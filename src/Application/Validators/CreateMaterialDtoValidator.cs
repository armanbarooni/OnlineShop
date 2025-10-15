using FluentValidation;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Validators
{
    public class CreateMaterialDtoValidator : AbstractValidator<CreateMaterialDto>
    {
        public CreateMaterialDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام متریال الزامی است")
                .MaximumLength(100).WithMessage("نام متریال نمی‌تواند بیش از 100 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
