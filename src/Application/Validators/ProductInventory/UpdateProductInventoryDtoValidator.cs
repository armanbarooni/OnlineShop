using FluentValidation;
using OnlineShop.Application.DTOs.ProductInventory;

namespace OnlineShop.Application.Validators.ProductInventory
{
    public class UpdateProductInventoryDtoValidator : AbstractValidator<UpdateProductInventoryDto>
    {
        public UpdateProductInventoryDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Product Inventory ID is required");

            RuleFor(x => x.AvailableQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AvailableQuantity.HasValue)
                .WithMessage("Available quantity cannot be negative");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.ReservedQuantity.HasValue)
                .WithMessage("Reserved quantity cannot be negative");

            RuleFor(x => x.SoldQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.SoldQuantity.HasValue)
                .WithMessage("Sold quantity cannot be negative");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Quantity.HasValue)
                .WithMessage("Quantity cannot be negative");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.CostPrice.HasValue)
                .WithMessage("Cost price cannot be negative");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.SellingPrice.HasValue)
                .WithMessage("Selling price cannot be negative");
        }
    }
}

