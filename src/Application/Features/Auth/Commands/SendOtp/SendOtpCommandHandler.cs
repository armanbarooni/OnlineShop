using MediatR;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Result<OtpResponseDto>>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly ISmsService _smsService;
        private readonly SmsSettings _smsSettings;

        public SendOtpCommandHandler(
            IOtpRepository otpRepository,
            ISmsService smsService,
            IOptions<SmsSettings> smsSettings)
        {
            _otpRepository = otpRepository;
            _smsService = smsService;
            _smsSettings = smsSettings.Value;
        }

        public async Task<Result<OtpResponseDto>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            // Invalidate all previous OTPs for this phone number
            await _otpRepository.InvalidatePreviousOtpsAsync(request.Request.PhoneNumber, cancellationToken);

            // Generate OTP code
            var code = GenerateOtpCode(_smsSettings.OtpLength);

            // Create OTP entity
            var otp = Otp.Create(
                request.Request.PhoneNumber,
                code,
                _smsSettings.OtpExpirationMinutes,
                request.Request.Purpose
            );

            // Save to database
            await _otpRepository.AddAsync(otp, cancellationToken);

            // Send SMS
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
            var random = new Random();
            var code = string.Empty;

            for (int i = 0; i < length; i++)
            { 
                code += random.Next(0, 10).ToString(); 
            }

            return code;
        }
    }
}

