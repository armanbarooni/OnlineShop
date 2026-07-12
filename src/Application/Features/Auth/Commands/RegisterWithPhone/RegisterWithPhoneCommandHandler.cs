using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Auth.Commands.RegisterWithPhone
{
    public class RegisterWithPhoneCommandHandler : IRequestHandler<RegisterWithPhoneCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpRepository _otpRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<RegisterWithPhoneCommandHandler> _logger;

        public RegisterWithPhoneCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOtpRepository otpRepository,
            ITokenService tokenService,
            ILogger<RegisterWithPhoneCommandHandler> logger)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterWithPhoneCommand request, CancellationToken cancellationToken)
        {
            var phoneNumber = request.Request.PhoneNumber;
            var receivedCode = request.Request.Code?.Trim() ?? string.Empty;
            var existingUser = await _userManager.FindByNameAsync(phoneNumber)
                ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);

            if (existingUser != null)
            {
                return Result<AuthResponseDto>.Failure("این شماره قبلاً ثبت‌نام شده است");
            }

            var otp = await _otpRepository.GetValidOtpByPhoneAsync(phoneNumber, cancellationToken);
            if (otp == null)
            {
                _logger.LogWarning(
                    "Phone registration OTP failed for {PhoneNumber}. No valid unused OTP was found.",
                    phoneNumber);
                return Result<AuthResponseDto>.Failure("کد تأیید نادرست است");
            }

            if (otp.Code != receivedCode)
            {
                _logger.LogWarning(
                    "Phone registration OTP failed for {PhoneNumber}. Expected: '{Expected}', Received: '{Received}', UsedFor: {UsedFor}, Attempts: {AttemptsCount}",
                    phoneNumber,
                    otp.Code,
                    receivedCode,
                    otp.UsedFor,
                    otp.AttemptsCount);
                return Result<AuthResponseDto>.Failure("کد تأیید نادرست است");
            }

            var user = new ApplicationUser
            {
                UserName = phoneNumber,
                Email = phoneNumber + "@phone.local",
                EmailConfirmed = false,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = true,
                FirstName = request.Request.FirstName ?? string.Empty,
                LastName = request.Request.LastName ?? string.Empty
            };

            IdentityResult result;
            if (!string.IsNullOrWhiteSpace(request.Request.Password))
            {
                result = await _userManager.CreateAsync(user, request.Request.Password);
            }
            else
            {
                result = await _userManager.CreateAsync(user);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure($"ثبت‌نام انجام نشد: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");

            otp.MarkAsUsed();
            await _otpRepository.UpdateAsync(otp, cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user.PhoneNumber!, roles);

            return Result<AuthResponseDto>.Success(tokens);
        }
    }
}
