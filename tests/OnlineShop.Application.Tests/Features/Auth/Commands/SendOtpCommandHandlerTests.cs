using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Features.Auth.Commands.SendOtp;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Tests.Features.Auth.Commands
{
    public class SendOtpCommandHandlerTests
    {
        private readonly Mock<IOtpRepository> _mockOtpRepository;
        private readonly Mock<ISmsService> _mockSmsService;
        private readonly IOptions<SmsSettings> _smsSettings;
        private readonly SendOtpCommandHandler _handler;

        public SendOtpCommandHandlerTests()
        {
            _mockOtpRepository = new Mock<IOtpRepository>();
            _mockSmsService = new Mock<ISmsService>();
            _smsSettings = Options.Create(new SmsSettings
            {
                OtpLength = 6,
                OtpExpirationMinutes = 5
            });

            _handler = new SendOtpCommandHandler(
                _mockOtpRepository.Object,
                _mockSmsService.Object,
                _smsSettings
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldSendOtpSuccessfully()
        {
            // Arrange
            var command = new SendOtpCommand
            {
                Request = new SendOtpDto
                {
                    PhoneNumber = "09123456789",
                    Purpose = "Login"
                }
            };

            _mockOtpRepository.Setup(r => r.InvalidatePreviousOtpsAsync(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockOtpRepository.Setup(r => r.AddAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns((Otp otp, CancellationToken ct) => Task.FromResult(otp));

            _mockSmsService.Setup(s => s.SendOtpAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Success.Should().BeTrue();
            result.Data.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));

            _mockOtpRepository.Verify(r => r.InvalidatePreviousOtpsAsync(
                "09123456789", 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockOtpRepository.Verify(r => r.AddAsync(
                It.Is<Otp>(o => o.PhoneNumber == "09123456789" && o.Code.Length == 6), 
                It.IsAny<CancellationToken>()), 
                Times.Once);

            _mockSmsService.Verify(s => s.SendOtpAsync(
                "09123456789", 
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_SmsServiceFails_ShouldReturnFailure()
        {
            // Arrange
            var command = new SendOtpCommand
            {
                Request = new SendOtpDto
                {
                    PhoneNumber = "09123456789",
                    Purpose = "Login"
                }
            };

            _mockOtpRepository.Setup(r => r.InvalidatePreviousOtpsAsync(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockOtpRepository.Setup(r => r.AddAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns((Otp otp, CancellationToken ct) => Task.FromResult(otp));

            _mockSmsService.Setup(s => s.SendOtpAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // SMS fails

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            result.ErrorMessage.Should().Contain("پیامک");
        }

        [Fact]
        public async Task Handle_InvalidatesAllPreviousOtps()
        {
            // Arrange
            var command = new SendOtpCommand
            {
                Request = new SendOtpDto
                {
                    PhoneNumber = "09123456789",
                    Purpose = "Register"
                }
            };

            _mockOtpRepository.Setup(r => r.InvalidatePreviousOtpsAsync(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockOtpRepository.Setup(r => r.AddAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Returns((Otp otp, CancellationToken ct) => Task.FromResult(otp));

            _mockSmsService.Setup(s => s.SendOtpAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockOtpRepository.Verify(r => r.InvalidatePreviousOtpsAsync(
                "09123456789", 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task Handle_GeneratesCorrectLengthOtpCode()
        {
            // Arrange
            var command = new SendOtpCommand
            {
                Request = new SendOtpDto
                {
                    PhoneNumber = "09123456789",
                    Purpose = "Login"
                }
            };

            Otp? capturedOtp = null;

            _mockOtpRepository.Setup(r => r.InvalidatePreviousOtpsAsync(
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockOtpRepository.Setup(r => r.AddAsync(
                It.IsAny<Otp>(), 
                It.IsAny<CancellationToken>()))
                .Callback<Otp, CancellationToken>((otp, ct) => capturedOtp = otp)
                .Returns((Otp otp, CancellationToken ct) => Task.FromResult(otp));

            _mockSmsService.Setup(s => s.SendOtpAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedOtp.Should().NotBeNull();
            capturedOtp!.Code.Should().HaveLength(6);
            capturedOtp.Code.Should().MatchRegex(@"^\d{6}$"); // 6 digits
        }
    }
}

