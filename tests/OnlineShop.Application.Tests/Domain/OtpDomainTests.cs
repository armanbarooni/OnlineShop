using Xunit;
using FluentAssertions;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Domain
{
    public class OtpDomainTests
    {
        [Fact]
        public void Create_ValidInputs_ShouldCreateOtp()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");

            otp.Should().NotBeNull();
            otp.PhoneNumber.Should().Be("09123456789");
            otp.Code.Should().Be("123456");
            otp.IsUsed.Should().BeFalse();
            otp.AttemptsCount.Should().Be(0);
        }

        [Fact]
        public void MarkAsUsed_ShouldSetIsUsedToTrue()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");

            otp.MarkAsUsed();

            otp.IsUsed.Should().BeTrue();
        }

        [Fact]
        public void IncrementAttempts_ShouldIncreaseAttemptsCount()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");

            otp.IncrementAttempts();
            otp.IncrementAttempts();

            otp.AttemptsCount.Should().Be(2);
        }

        [Fact]
        public void IsExpired_BeforeExpiryTime_ShouldReturnFalse()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");

            otp.IsExpired().Should().BeFalse();
        }

        [Fact]
        public void HasExceededMaxAttempts_WithinLimit_ShouldReturnFalse()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");
            otp.IncrementAttempts();
            otp.IncrementAttempts();

            otp.HasExceededMaxAttempts(5).Should().BeFalse();
        }

        [Fact]
        public void HasExceededMaxAttempts_ExceedsLimit_ShouldReturnTrue()
        {
            var otp = Otp.Create("09123456789", "123456", 5, "Login");
            for (int i = 0; i < 6; i++)
            {
                otp.IncrementAttempts();
            }

            otp.HasExceededMaxAttempts(5).Should().BeTrue();
        }
    }
}

