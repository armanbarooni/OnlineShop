using FluentValidation;
using OnlineShop.Application.DTOs.Unit;

namespace OnlineShop.Application.Validators.Unit
{
    public class CreateUnitDtoValidator : AbstractValidator<CreateUnitDto>
    {
        public CreateUnitDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.");

            RuleFor(x => x.Comment)
                .MaximumLength(200).WithMessage("Comment must not exceed 200 characters.");
        }
    }
}
