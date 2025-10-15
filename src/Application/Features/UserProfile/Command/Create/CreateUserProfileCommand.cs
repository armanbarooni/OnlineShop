using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Command.Create
{
    public class CreateUserProfileCommand : IRequest<Result<UserProfileDto>>
    {
        public CreateUserProfileDto? UserProfile { get; set; }
    }
}

