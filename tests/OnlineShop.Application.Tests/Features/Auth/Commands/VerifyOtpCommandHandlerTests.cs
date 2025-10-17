using Xunit;
using Moq;
using FluentAssertions;
using OnlineShop.Application.Features.Auth.Commands.VerifyOtp;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Common.Models;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Auth.Commands
{
    public class VerifyOtpCommandHandlerTests
    {
        private readonly Mock<IOtpRepository> _mockOtpRepository;
        private readonly VerifyOtpCommandHandler _handler;

        public VerifyOtpCommandHandlerTests()
        {
            _mockOtpRepository = new Mock<IOtpRepository>();
            _handler = new VerifyOtpCommandHandler(_mockOtpRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidOtp_ShouldReturnSuccess()
        {
            // Arrange
            var phoneNumber = "09123456789";
            var code = "123456";

            var otp = Otp.Create(phoneNumber, code, 5, "Login");

            _mockOtpRepository.Setup(r => r.GetValidOtpByPhoneAsync(
                phoneNumber, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(otp);

            _mockOtpRepository.Setup(r => r.UpdateAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new VerifyOtpCommand
            {
                Request = new VerifyOtpDto
                {
                    PhoneNumber = phoneNumber,
                    Code = code
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Success.Should().BeTrue();

            _mockOtpRepository.Verify(r => r.UpdateAsync(
                It.Is<Otp>(o => o.IsUsed), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidCode_ShouldReturnFailure()
        {
            // Arrange
            var phoneNumber = "09123456789";
            var correctCode = "123456";
            var incorrectCode = "999999";

            var otp = Otp.Create(phoneNumber, correctCode, 5, "Login");

            _mockOtpRepository.Setup(r => r.GetValidOtpByPhoneAsync(
                phoneNumber, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(otp);

            _mockOtpRepository.Setup(r => r.UpdateAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new VerifyOtpCommand
            {
                Request = new VerifyOtpDto
                {
                    PhoneNumber = phoneNumber,
                    Code = incorrectCode
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("نادرست");
        }

        [Fact]
        public async Task Handle_ExpiredOtp_ShouldReturnFailure()
        {
            // Arrange
            var phoneNumber = "09123456789";

            _mockOtpRepository.Setup(r => r.GetValidOtpByPhoneAsync(
                phoneNumber, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((Otp?)null); // No valid OTP

            var command = new VerifyOtpCommand
            {
                Request = new VerifyOtpDto
                {
                    PhoneNumber = phoneNumber,
                    Code = "123456"
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("منقضی");
        }

        [Fact]
        public async Task Handle_TooManyAttempts_ShouldDeleteOtpAndReturnFailure()
        {
            // Arrange
            var phoneNumber = "09123456789";
            var code = "123456";

            var otp = Otp.Create(phoneNumber, code, 5, "Login");
            // Simulate 5 failed attempts
            for (int i = 0; i < 5; i++)
            {
                otp.IncrementAttempts();
            }

            _mockOtpRepository.Setup(r => r.GetValidOtpByPhoneAsync(
                phoneNumber, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(otp);

            _mockOtpRepository.Setup(r => r.UpdateAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new VerifyOtpCommand
            {
                Request = new VerifyOtpDto
                {
                    PhoneNumber = phoneNumber,
                    Code = "wrong"
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("تلاش");

            _mockOtpRepository.Verify(r => r.UpdateAsync(
                It.Is<Otp>(o => o.Deleted), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_IncrementsAttemptsOnWrongCode()
        {
            // Arrange
            var phoneNumber = "09123456789";
            var correctCode = "123456";
            var incorrectCode = "wrong";

            var otp = Otp.Create(phoneNumber, correctCode, 5, "Login");
            var initialAttempts = otp.AttemptsCount;

            _mockOtpRepository.Setup(r => r.GetValidOtpByPhoneAsync(
                phoneNumber, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(otp);

            _mockOtpRepository.Setup(r => r.UpdateAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new VerifyOtpCommand
            {
                Request = new VerifyOtpDto
                {
                    PhoneNumber = phoneNumber,
                    Code = incorrectCode
                }
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            otp.AttemptsCount.Should().Be(initialAttempts + 1);

            _mockOtpRepository.Verify(r => r.UpdateAsync(
                It.Is<Otp>(o => o.AttemptsCount > initialAttempts), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
    }
}

