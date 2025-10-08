using FluentAssertions;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Validators.Unit;
using Xunit;

namespace OnlineShop.Application.Tests.Features.Unit.Commands.Update
{
    public class UpdateUnitCommandValidatorTests
    {
        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var validator = new UpdateUnitDtoValidator();
            var dto = new UpdateUnitDto { Id = Guid.NewGuid(), Name = "", Comment = "c" };
            var result = validator.Validate(dto);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Should_Pass_When_Valid()
        {
            var validator = new UpdateUnitDtoValidator();
            var dto = new UpdateUnitDto { Id = Guid.NewGuid(), Name = "Kilogram", Comment = "Weight" };
            var result = validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }
    }
}


