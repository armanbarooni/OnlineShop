using Xunit;
using FluentAssertions;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Domain
{
    public class MaterialDomainTests
    {
        [Fact]
        public void Create_ValidInputs_ShouldCreateMaterial()
        {
            var material = Material.Create("Cotton", "100% Cotton");

            material.Should().NotBeNull();
            material.Name.Should().Be("Cotton");
            material.Description.Should().Be("100% Cotton");
            material.IsActive.Should().BeTrue();
        }

        [Fact]
        public void SetName_ValidName_ShouldUpdateName()
        {
            var material = Material.Create("Cotton", null);

            material.SetName("Polyester");

            material.Name.Should().Be("Polyester");
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var material = Material.Create("Cotton", null);

            material.Deactivate();

            material.IsActive.Should().BeFalse();
        }
    }
}

