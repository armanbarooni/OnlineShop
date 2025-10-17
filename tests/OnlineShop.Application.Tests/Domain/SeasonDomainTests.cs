using Xunit;
using FluentAssertions;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Domain
{
    public class SeasonDomainTests
    {
        [Fact]
        public void Create_ValidInputs_ShouldCreateSeason()
        {
            var season = Season.Create("Spring", "SPRING");

            season.Should().NotBeNull();
            season.Name.Should().Be("Spring");
            season.Code.Should().Be("SPRING");
            season.IsActive.Should().BeTrue();
        }

        [Fact]
        public void SetName_ValidName_ShouldUpdateName()
        {
            var season = Season.Create("Spring", "SPRING");

            season.SetName("Summer");

            season.Name.Should().Be("Summer");
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var season = Season.Create("Spring", "SPRING");

            season.Deactivate();

            season.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Update_ValidInputs_ShouldUpdateSeasonProperties()
        {
            var season = Season.Create("Spring", "SPRING");

            season.Update("Summer", "SUMMER", "Admin");

            season.Name.Should().Be("Summer");
            season.Code.Should().Be("SUMMER");
        }
    }
}

