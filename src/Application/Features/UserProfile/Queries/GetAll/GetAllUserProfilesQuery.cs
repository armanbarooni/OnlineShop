using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Queries.GetAll
{
    public class GetAllUserProfilesQuery : IRequest<Result<List<UserProfileDto>>>
    {
    }
}


