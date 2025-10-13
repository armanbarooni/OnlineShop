using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Persistence.InterFaces.Repositories;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<OtpResponseDto>>
    {
        private readonly IOtpRepository _otpRepository;
        private const int MAX_ATTEMPTS = 5;

        public VerifyOtpCommandHandler(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<Result<OtpResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            // Get valid OTP for phone number
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(request.Request.PhoneNumber, cancellationToken);

            if (otp == null)
            {
                return Result<OtpResponseDto>.Failure("کد تایید یافت نشد یا منقضی شده است");
            }

            // Check if too many attempts
            if (otp.HasExceededMaxAttempts(MAX_ATTEMPTS))
            {
                otp.Delete();
                await _otpRepository.UpdateAsync(otp, cancellationToken);
                return Result<OtpResponseDto>.Failure("تعداد تلاش‌های مجاز تمام شده است. لطفاً کد جدید درخواست کنید");
            }

            // Verify code
            if (otp.Code != request.Request.Code)
            {
                otp.IncrementAttempts();
                await _otpRepository.UpdateAsync(otp, cancellationToken);
                
                var remainingAttempts = MAX_ATTEMPTS - otp.AttemptsCount;
                return Result<OtpResponseDto>.Failure($"کد تایید نادرست است. {remainingAttempts} تلاش باقی مانده");
            }

            // Mark OTP as used
            otp.MarkAsUsed();
            await _otpRepository.UpdateAsync(otp, cancellationToken);

            var response = new OtpResponseDto
            {
                Success = true,
                Message = "کد تایید با موفقیت تأیید شد"
            };

            return Result<OtpResponseDto>.Success(response);
        }
    }
}

