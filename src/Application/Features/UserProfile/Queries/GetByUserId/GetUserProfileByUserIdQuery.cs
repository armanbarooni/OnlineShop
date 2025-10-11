using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Queries.GetByUserId
{
    public class GetUserProfileByUserIdQuery : IRequest<Result<UserProfileDto?>>
    {
        public Guid UserId { get; set; }
    }
}
