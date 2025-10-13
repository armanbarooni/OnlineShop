using FluentValidation;
using OnlineShop.Application.DTOs.UserAddress;
using System.Text.RegularExpressions;

namespace OnlineShop.Application.Validators.UserAddress
{
    public class UpdateUserAddressDtoValidator : AbstractValidator<UpdateUserAddressDto>
    {
        public UpdateUserAddressDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("User Address ID is required");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Address title is required")
                .MaximumLength(50)
                .WithMessage("Address title cannot exceed 50 characters");

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

            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                .WithMessage("Address line 1 is required")
                .MaximumLength(200)
                .WithMessage("Address line 1 cannot exceed 200 characters");

            RuleFor(x => x.AddressLine2)
                .MaximumLength(200)
                .WithMessage("Address line 2 cannot exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required")
                .MaximumLength(100)
                .WithMessage("City cannot exceed 100 characters");

            RuleFor(x => x.State)
                .NotEmpty()
                .WithMessage("State is required")
                .MaximumLength(100)
                .WithMessage("State cannot exceed 100 characters");

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required")
                .MaximumLength(20)
                .WithMessage("Postal code cannot exceed 20 characters")
                .Matches(@"^\d{10}$")
                .WithMessage("Postal code must be 10 digits for Iran");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Country is required")
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters")
                .Matches(@"^09\d{9}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be a valid Iranian mobile number (09XXXXXXXXX)");
        }
    }
}

