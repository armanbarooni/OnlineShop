using FluentValidation;
using OnlineShop.Application.DTOs.Unit;

namespace OnlineShop.Application.Validators.Unit
{
    public class UpdateUnitDtoValidator : AbstractValidator<UpdateUnitDto>
    {
        public UpdateUnitDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Unit ID is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Unit name is required.")
                .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.");

            RuleFor(x => x.Comment)
                .MaximumLength(200).WithMessage("Comment must not exceed 200 characters.");
        }
    }
}
