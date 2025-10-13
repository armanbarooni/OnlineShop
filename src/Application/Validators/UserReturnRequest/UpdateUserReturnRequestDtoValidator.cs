using FluentValidation;
using OnlineShop.Application.DTOs.UserReturnRequest;

namespace OnlineShop.Application.Validators.UserReturnRequest
{
    public class UpdateUserReturnRequestDtoValidator : AbstractValidator<UpdateUserReturnRequestDto>
    {
        public UpdateUserReturnRequestDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Return Request ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.ReturnReason)
                .NotEmpty()
                .WithMessage("Return reason is required")
                .MaximumLength(500)
                .WithMessage("Return reason cannot exceed 500 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.RefundAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Refund amount cannot be negative");
        }
    }
}

