using FluentValidation;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Validators.UserReturnRequest
{
    public class ApproveUserReturnRequestDtoValidator : AbstractValidator<ApproveUserReturnRequestDto>
    {
        public ApproveUserReturnRequestDtoValidator()
        {
            RuleFor(x => x.AdminNotes)
                .MaximumLength(1000)
                .WithMessage("Admin notes cannot exceed 1000 characters");
        }
    }
}

