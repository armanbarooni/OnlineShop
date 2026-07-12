using MediatR;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<OtpResponseDto>>
    {
        private const int MaxAttempts = 3;

        private readonly IOtpRepository _otpRepository;
        private readonly ILogger<VerifyOtpCommandHandler> _logger;

        public VerifyOtpCommandHandler(IOtpRepository otpRepository, ILogger<VerifyOtpCommandHandler> logger)
        {
            _otpRepository = otpRepository;
            _logger = logger;
        }

        public async Task<Result<OtpResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(request.Request.PhoneNumber, cancellationToken);

            if (otp == null)
            {
                return Result<OtpResponseDto>.Failure("کد تأیید یافت نشد یا منقضی شده است");
            }

            if (otp.HasExceededMaxAttempts(MaxAttempts))
            {
                otp.Delete();
                await _otpRepository.UpdateAsync(otp, cancellationToken);
                return Result<OtpResponseDto>.Failure("تعداد تلاش‌های مجاز تمام شده است. لطفاً کد جدید درخواست کنید");
            }

            if (otp.Code != request.Request.Code)
            {
                _logger.LogWarning(
                    "OTP verification failed for {PhoneNumber}. Expected: '{Expected}', Received: '{Received}'.",
                    request.Request.PhoneNumber,
                    otp.Code,
                    request.Request.Code);

                otp.IncrementAttempts();
                await _otpRepository.UpdateAsync(otp, cancellationToken);

                var remainingAttempts = MaxAttempts - otp.AttemptsCount;
                return Result<OtpResponseDto>.Failure($"کد تأیید نادرست است. {remainingAttempts} تلاش باقی مانده");
            }

            otp.MarkAsUsed();
            await _otpRepository.UpdateAsync(otp, cancellationToken);

            var response = new OtpResponseDto
            {
                Success = true,
                Message = "کد تأیید با موفقیت تأیید شد"
            };

            return Result<OtpResponseDto>.Success(response);
        }
    }
}
