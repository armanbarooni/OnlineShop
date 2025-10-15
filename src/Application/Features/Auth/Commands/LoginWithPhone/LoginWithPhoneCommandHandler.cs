using MediatR;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;

using OnlineShop.Domain.Interfaces.Repositories;
namespace OnlineShop.Application.Features.Auth.Commands.LoginWithPhone
{
    public class LoginWithPhoneCommandHandler : IRequestHandler<LoginWithPhoneCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpRepository _otpRepository;
        private readonly ITokenService _tokenService;

        public LoginWithPhoneCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOtpRepository otpRepository,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginWithPhoneCommand request, CancellationToken cancellationToken)
        {
            // Verify OTP first
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(request.Request.PhoneNumber, cancellationToken);

            if (otp == null || otp.Code != request.Request.Code)
            {
                return Result<AuthResponseDto>.Failure("کد تایید نامعتبر یا منقضی شده است");
            }

            // Find user by phone number
            var user = await _userManager.FindByNameAsync(request.Request.PhoneNumber);
            if (user == null)
            {
                return Result<AuthResponseDto>.Failure("کاربری با این شماره تلفن یافت نشد. لطفاً ابتدا ثبت‌نام کنید");
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Mark OTP as used
            otp.MarkAsUsed();
            await _otpRepository.UpdateAsync(otp, cancellationToken);

            // Generate tokens
            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user.PhoneNumber!, roles);

            return Result<AuthResponseDto>.Success(tokens);
        }
    }
}



