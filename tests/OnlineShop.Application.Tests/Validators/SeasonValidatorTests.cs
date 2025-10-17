using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using OnlineShop.Application.Validators;
using OnlineShop.Application.DTOs.Season;

namespace OnlineShop.Application.Tests.Validators
{
    public class SeasonValidatorTests
    {
        [Fact]
        public void CreateSeasonValidator_ValidDto_ShouldPass()
        {
            var validator = new CreateSeasonDtoValidator();
            var dto = new CreateSeasonDto { Name = "Spring", Code = "SPRING" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void CreateSeasonValidator_EmptyName_ShouldFail()
        {
            var validator = new CreateSeasonDtoValidator();
            var dto = new CreateSeasonDto { Name = "", Code = "SPRING" };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void UpdateSeasonValidator_ValidDto_ShouldPass()
        {
            var validator = new UpdateSeasonDtoValidator();
            var dto = new UpdateSeasonDto { Id = Guid.NewGuid(), Name = "Spring", Code = "SPRING" };

            var result = validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}

