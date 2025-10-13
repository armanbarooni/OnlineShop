using FluentValidation;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Validators.UserProfile
{
    public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
    {
        public UpdateUserProfileDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Profile ID is required");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.NationalCode)
                .Length(10)
                .When(x => !string.IsNullOrEmpty(x.NationalCode))
                .WithMessage("National code must be exactly 10 digits")
                .Matches(@"^\d{10}$")
                .When(x => !string.IsNullOrEmpty(x.NationalCode))
                .WithMessage("National code must contain only digits");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Now)
                .When(x => x.BirthDate.HasValue)
                .WithMessage("Birth date must be in the past");

            RuleFor(x => x.Gender)
                .Must(BeValidGender)
                .When(x => !string.IsNullOrEmpty(x.Gender))
                .WithMessage("Invalid gender. Valid values: Male, Female, Other");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500)
                .WithMessage("Profile image URL cannot exceed 500 characters")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("Profile image URL must be a valid URL");

            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .WithMessage("Bio cannot exceed 1000 characters");

            RuleFor(x => x.Website)
                .MaximumLength(200)
                .WithMessage("Website cannot exceed 200 characters")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrEmpty(x.Website))
                .WithMessage("Website must be a valid URL");
        }

        private static bool BeValidGender(string? gender)
        {
            if (string.IsNullOrEmpty(gender)) return true;
            var validGenders = new[] { "Male", "Female", "Other" };
            return validGenders.Contains(gender);
        }

        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}

