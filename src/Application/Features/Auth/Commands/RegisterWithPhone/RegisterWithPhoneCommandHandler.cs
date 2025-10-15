using MediatR;
using Microsoft.AspNetCore.Identity;
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

        public RegisterWithPhoneCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOtpRepository otpRepository,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterWithPhoneCommand request, CancellationToken cancellationToken)
        {
            // Verify OTP first
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(request.Request.PhoneNumber, cancellationToken);

            if (otp == null || otp.Code != request.Request.Code)
            {
                return Result<AuthResponseDto>.Failure("کد تایید نامعتبر یا منقضی شده است");
            }

            // Check if user already exists with this phone number
            var existingUser = await _userManager.FindByNameAsync(request.Request.PhoneNumber);
            if (existingUser != null)
            {
                return Result<AuthResponseDto>.Failure("این شماره تلفن قبلاً ثبت شده است");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Request.PhoneNumber,
                Email = request.Request.PhoneNumber + "@phone.local", // Temporary email for phone-only registration
                EmailConfirmed = false,
                PhoneNumber = request.Request.PhoneNumber,
                PhoneNumberConfirmed = true, // Phone is verified via OTP
                FirstName = request.Request.FirstName ?? string.Empty,
                LastName = request.Request.LastName ?? string.Empty
            };

            IdentityResult result;
            
            // If password provided, create with password
            if (!string.IsNullOrWhiteSpace(request.Request.Password))
            {
                result = await _userManager.CreateAsync(user, request.Request.Password);
            }
            else
            {
                // Create without password (user will always login with OTP)
                result = await _userManager.CreateAsync(user);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure($"خطا در ثبت‌نام: {errors}");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

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



