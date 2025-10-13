using FluentValidation;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Validators.UserReturnRequest
{
    public class RejectUserReturnRequestDtoValidator : AbstractValidator<RejectUserReturnRequestDto>
    {
        public RejectUserReturnRequestDtoValidator()
        {
            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required")
                .MaximumLength(500)
                .WithMessage("Rejection reason cannot exceed 500 characters");

            RuleFor(x => x.AdminNotes)
                .MaximumLength(1000)
                .WithMessage("Admin notes cannot exceed 1000 characters");
        }
    }
}

