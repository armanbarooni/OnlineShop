using FluentValidation.TestHelper;
using Xunit;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Validators.ProductCategory;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Validators
{
    public class UpdateProductCategoryDtoValidatorTests
    {
        private readonly UpdateProductCategoryDtoValidator _validator;

        public UpdateProductCategoryDtoValidatorTests()
        {
            _validator = new UpdateProductCategoryDtoValidator();
        }

        [Fact]
        public void Validate_ValidDto_ReturnsValid()
        {
            // Arrange
            var dto = new UpdateProductCategoryDto
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_EmptyName_ReturnsInvalid()
        {
            // Arrange
            var dto = new UpdateProductCategoryDto
            {
                Name = "",
                Description = "Description"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Fact]
        public void Validate_NameExceeds100Characters_ReturnsInvalid()
        {
            // Arrange
            var dto = new UpdateProductCategoryDto
            {
                Name = new string('A', 101),
                Description = "Description"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
        }

        [Fact]
        public void Validate_DescriptionExceeds500Characters_ReturnsInvalid()
        {
            // Arrange
            var dto = new UpdateProductCategoryDto
            {
                Name = "Category",
                Description = new string('A', 501)
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("500"));
        }
    }
}
