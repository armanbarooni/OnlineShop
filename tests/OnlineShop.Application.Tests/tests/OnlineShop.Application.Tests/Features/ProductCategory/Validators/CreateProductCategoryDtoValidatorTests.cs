using FluentValidation.TestHelper;
using Xunit;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Validators.ProductCategory;

namespace OnlineShop.Application.Tests.Features.ProductCategory.Validators
{
    public class CreateProductCategoryDtoValidatorTests
    {
        private readonly CreateProductCategoryDtoValidator _validator;

        public CreateProductCategoryDtoValidatorTests()
        {
            _validator = new CreateProductCategoryDtoValidator();
        }

        [Fact]
        public void Validate_ValidDto_ReturnsValid()
        {
            // Arrange
            var dto = new CreateProductCategoryDto
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
            var dto = new CreateProductCategoryDto
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
            var dto = new CreateProductCategoryDto
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
            var dto = new CreateProductCategoryDto
            {
                Name = "Electronics",
                Description = new string('A', 501)
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("500"));
        }

        [Fact]
        public void Validate_EmptyDescription_ReturnsValid()
        {
            // Arrange
            var dto = new CreateProductCategoryDto
            {
                Name = "Electronics",
                Description = ""
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
