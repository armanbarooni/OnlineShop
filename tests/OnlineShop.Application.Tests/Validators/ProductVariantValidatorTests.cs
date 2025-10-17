using Xunit;
using FluentValidation.TestHelper;
using OnlineShop.Application.Validators;
using OnlineShop.Application.DTOs.ProductVariant;

namespace OnlineShop.Application.Tests.Validators
{
    public class ProductVariantValidatorTests
    {
        [Fact]
        public void CreateProductVariantValidator_ValidDto_ShouldPass()
        {
            var validator = new CreateProductVariantDtoValidator();
            var dto = new CreateProductVariantDto 
            { 
                ProductId = Guid.NewGuid(),
                Size = "M",
                Color = "Red",
                SKU = "VAR-001",
                StockQuantity = 100
            };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreateProductVariantValidator_EmptySize_ShouldFail()
        {
            var validator = new CreateProductVariantDtoValidator();
            var dto = new CreateProductVariantDto 
            { 
                ProductId = Guid.NewGuid(),
                Size = "",
                Color = "Red",
                SKU = "VAR-001",
                StockQuantity = 100
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Size);
        }

    }
}

