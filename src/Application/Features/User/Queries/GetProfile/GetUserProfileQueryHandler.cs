using MediatR;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Features.User.Queries.GetProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUserProfileQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<UserProfileDto>.Failure("کاربر یافت نشد");
            }

            return Result<UserProfileDto>.Success(new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? "",
                Email = user.Email
            });
        }
    }
}
