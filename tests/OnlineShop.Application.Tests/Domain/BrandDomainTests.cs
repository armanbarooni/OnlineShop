using Xunit;
using FluentAssertions;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Domain
{
    public class BrandDomainTests
    {
        [Fact]
        public void Create_ValidInputs_ShouldCreateBrand()
        {
            var brand = Brand.Create("Nike", "https://logo.png", "Just Do It");

            brand.Should().NotBeNull();
            brand.Name.Should().Be("Nike");
            brand.LogoUrl.Should().Be("https://logo.png");
            brand.Description.Should().Be("Just Do It");
            brand.IsActive.Should().BeTrue();
        }

        [Fact]
        public void SetName_ValidName_ShouldUpdateName()
        {
            var brand = Brand.Create("Nike", null, null);

            brand.SetName("Adidas");

            brand.Name.Should().Be("Adidas");
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var brand = Brand.Create("Nike", null, null);

            brand.Deactivate();

            brand.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var brand = Brand.Create("Nike", null, null);
            brand.Deactivate();

            brand.Activate();

            brand.IsActive.Should().BeTrue();
        }
    }
}

