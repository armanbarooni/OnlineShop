using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;

namespace OnlineShop.Application.Features.User.Queries.GetProfile
{
    public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
    {
        public Guid UserId { get; set; }
    }
}
