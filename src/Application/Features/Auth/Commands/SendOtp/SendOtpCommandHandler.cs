using MediatR;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<SendOtpCommandHandler> _logger;

        public SendOtpCommandHandler(
            IOtpRepository otpRepository,
            ISmsService smsService,
            UserManager<ApplicationUser> userManager,
            IOptions<SmsSettings> smsSettings,
            ILogger<SendOtpCommandHandler> logger)
        {
            _otpRepository = otpRepository;
            _smsService = smsService;
            _userManager = userManager;
            _smsSettings = smsSettings.Value;
            _logger = logger;
        }

        public async Task<Result<OtpResponseDto>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            if (string.Equals(request.Request.Purpose, "Login", StringComparison.OrdinalIgnoreCase))
            {
                var userExists = _userManager.Users.Any(u => u.PhoneNumber == request.Request.PhoneNumber);

                if (!userExists)
                {
                    return Result<OtpResponseDto>.Failure("کاربری با این شماره یافت نشد");
                }
            }

            var lastOtp = await _otpRepository.GetLatestOtpAsync(request.Request.PhoneNumber, cancellationToken);
            if (lastOtp != null &&
                !lastOtp.IsUsed &&
                string.Equals(lastOtp.UsedFor, request.Request.Purpose, StringComparison.OrdinalIgnoreCase))
            {
                var timeSinceLastOtp = DateTime.UtcNow - lastOtp.CreatedAt;
                const int rateLimitMinutes = 2;

                if (timeSinceLastOtp.TotalMinutes < rateLimitMinutes)
                {
                    _logger.LogWarning(
                        "OTP rate limit exceeded for {PhoneNumber}. Last OTP sent {Seconds} seconds ago",
                        request.Request.PhoneNumber,
                        (int)timeSinceLastOtp.TotalSeconds);

                    return Result<OtpResponseDto>.Failure("پیامک برای شما اخیراً ارسال شده است، لطفاً حداقل ۲ دقیقه صبر کنید");
                }
            }

            await _otpRepository.InvalidatePreviousOtpsAsync(request.Request.PhoneNumber, cancellationToken);

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
