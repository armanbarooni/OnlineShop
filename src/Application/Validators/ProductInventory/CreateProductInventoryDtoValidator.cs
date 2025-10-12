using FluentValidation;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Validators.ProductInventory
{
    public class CreateProductInventoryDtoValidator : AbstractValidator<CreateProductInventoryDto>
    {
        public CreateProductInventoryDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("Product ID is required");

            RuleFor(x => x.UnitId)
                .NotEmpty()
                .WithMessage("Unit ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Reserved quantity cannot be negative");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Unit price must be greater than 0");

            RuleFor(x => x.Location)
                .MaximumLength(200)
                .WithMessage("Location cannot exceed 200 characters");
        }
    }
}
