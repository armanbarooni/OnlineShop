using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Result<OtpResponseDto>>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly ISmsService _smsService;
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<SendOtpCommandHandler> _logger;

        public SendOtpCommandHandler(
            IOtpRepository otpRepository,
            ISmsService smsService,
            IOptions<SmsSettings> smsSettings,
            ILogger<SendOtpCommandHandler> logger)
        {
            _otpRepository = otpRepository;
            _smsService = smsService;
            _smsSettings = smsSettings.Value;
            _logger = logger;
        }

        public async Task<Result<OtpResponseDto>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var lastOtp = await _otpRepository.GetLatestOtpAsync(request.Request.PhoneNumber, cancellationToken);
            if (lastOtp != null && !lastOtp.IsUsed)
            {
                var timeSinceLastOtp = DateTime.UtcNow - lastOtp.CreatedAt;
                const int rateLimitMinutes = 2;

                if (timeSinceLastOtp.TotalMinutes < rateLimitMinutes)
                {
                    var remainingSeconds = (int)((rateLimitMinutes * 60) - timeSinceLastOtp.TotalSeconds);
                    _logger.LogWarning(
                        "OTP rate limit exceeded for {PhoneNumber}. Last OTP sent {Seconds} seconds ago",
                        request.Request.PhoneNumber,
                        (int)timeSinceLastOtp.TotalSeconds);

                    return Result<OtpResponseDto>.Failure(
                        $"لطفاً {remainingSeconds} ثانیه دیگر صبر کنید و سپس دوباره تلاش کنید");
                }
            }

            await _otpRepository.InvalidatePreviousOtpsAsync(request.Request.PhoneNumber, cancellationToken);

            if (string.Equals(request.Request.Purpose, "Login", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "SendOtp requested for login flow. Continuing to send OTP for {PhoneNumber}",
                    request.Request.PhoneNumber);
            }

            var code = GenerateOtpCode(_smsSettings.OtpLength);
            _logger.LogInformation(
                "Generated OTP: {Code} with length {Length} for {PhoneNumber}",
                code,
                code.Length,
                request.Request.PhoneNumber);

            var otp = Otp.Create(
                request.Request.PhoneNumber,
                code,
                _smsSettings.OtpExpirationMinutes,
                request.Request.Purpose);

            await _otpRepository.AddAsync(otp, cancellationToken);

            var smsSent = await _smsService.SendOtpAsync(request.Request.PhoneNumber, code, cancellationToken);
            if (!smsSent)
            {
                return Result<OtpResponseDto>.Failure("خطا در ارسال پیامک. لطفاً دوباره تلاش کنید");
            }

            var response = new OtpResponseDto
            {
                Success = true,
                Message = $"کد تایید به شماره {request.Request.PhoneNumber} ارسال شد",
                ExpiresAt = otp.ExpiresAt
            };

            return Result<OtpResponseDto>.Success(response);
        }

        private string GenerateOtpCode(int length)
        {
            if (length < 4)
            {
                length = 4;
            }

            var code = string.Empty;
            for (var i = 0; i < length; i++)
            {
                code += Random.Shared.Next(0, 10).ToString();
            }

            return code;
        }
    }
}
