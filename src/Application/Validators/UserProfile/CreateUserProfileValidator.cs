using FluentValidation;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Validators.UserProfile
{
    public class CreateUserProfileValidator : AbstractValidator<CreateUserProfileDto>
    {
        public CreateUserProfileValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.NationalCode)
                .Length(10)
                .When(x => !string.IsNullOrEmpty(x.NationalCode))
                .WithMessage("National code must be exactly 10 digits");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.UtcNow)
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Birth date must be in the past");

            RuleFor(x => x.Gender)
                .Must(g => string.IsNullOrEmpty(g) || new[] { "Male", "Female", "Other" }.Contains(g))
                .WithMessage("Gender must be Male, Female, or Other");

            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .WithMessage("Bio cannot exceed 1000 characters");

            RuleFor(x => x.Website)
                .MaximumLength(200)
                .WithMessage("Website cannot exceed 200 characters");
        }
    }
}
