using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using OnlineShop.Application.Validators;
using OnlineShop.Application.DTOs.Material;

namespace OnlineShop.Application.Tests.Validators
{
    public class MaterialValidatorTests
    {
        [Fact]
        public void CreateMaterialValidator_ValidDto_ShouldPass()
        {
            var validator = new CreateMaterialDtoValidator();
            var dto = new CreateMaterialDto { Name = "Cotton" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreateMaterialValidator_EmptyName_ShouldFail()
        {
            var validator = new CreateMaterialDtoValidator();
            var dto = new CreateMaterialDto { Name = "" };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void UpdateMaterialValidator_ValidDto_ShouldPass()
        {
            var validator = new UpdateMaterialDtoValidator();
            var dto = new UpdateMaterialDto { Id = Guid.NewGuid(), Name = "Cotton" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

