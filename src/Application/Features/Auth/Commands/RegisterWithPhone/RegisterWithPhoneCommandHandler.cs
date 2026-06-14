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
            // TEMPORARY: OTP verification disabled for testing
            // TODO: Re-enable OTP verification before production deployment

            /* COMMENTED OUT - OTP VERIFICATION
            var otp = await _otpRepository.GetValidOtpByPhoneAsync(request.Request.PhoneNumber, cancellationToken);
            if (otp == null || otp.Code != request.Request.Code)
            {
                return Result<AuthResponseDto>.Failure("کد تایید نامعتبر یا منقضی شده است");
            }
            */

            var existingUser = await _userManager.FindByNameAsync(request.Request.PhoneNumber);
            if (existingUser != null)
            {
                return Result<AuthResponseDto>.Failure("کاربری با این شماره موبایل قبلاً ثبت‌نام کرده است");
            }

            var user = new ApplicationUser
            {
                UserName = request.Request.PhoneNumber,
                Email = request.Request.PhoneNumber + "@phone.local",
                EmailConfirmed = false,
                PhoneNumber = request.Request.PhoneNumber,
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

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user.PhoneNumber!, roles);

            return Result<AuthResponseDto>.Success(tokens);
        }
    }
}
