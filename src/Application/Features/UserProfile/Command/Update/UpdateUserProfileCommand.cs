using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Command.Update
{
    public class UpdateUserProfileCommand : IRequest<Result<UserProfileDto>>
    {
        public UpdateUserProfileDto? UserProfile { get; set; }
    }
}

