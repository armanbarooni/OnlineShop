using MediatR;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.User.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateProfileCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<string>.Failure("کاربر یافت نشد");
            }

            user.FirstName = request.Request.FirstName;
            user.LastName = request.Request.LastName;
            user.Email = request.Request.Email;
            // Optionally update UserName if Email is used as UserName, but usually UserName is phone or email.
            // If UserName is phone, we don't change it here unless phone is updated via OTP.

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Result<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return Result<string>.Success("پروفایل با موفقیت بروزرسانی شد");
        }
    }
}
