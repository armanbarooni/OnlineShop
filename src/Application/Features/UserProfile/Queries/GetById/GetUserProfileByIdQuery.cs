using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserProfile;

namespace OnlineShop.Application.Features.UserProfile.Queries.GetById
{
    public class GetUserProfileByIdQuery : IRequest<Result<UserProfileDto>>
    {
        public Guid Id { get; set; }
    }
}


