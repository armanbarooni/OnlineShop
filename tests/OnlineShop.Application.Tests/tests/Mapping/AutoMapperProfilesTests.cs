using AutoMapper;
using FluentAssertions;
using OnlineShop.Application;
using Xunit;

namespace OnlineShop.Application.Tests.Mapping
{
    public class AutoMapperProfilesTests
    {
        [Fact]
        public void AutoMapper_Profiles_Should_BeValid()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(AssemblyReference).Assembly);
            });

            // Act & Assert
            config.Invoking(c => c.AssertConfigurationIsValid()).Should().NotThrow();
        }
    }
}


