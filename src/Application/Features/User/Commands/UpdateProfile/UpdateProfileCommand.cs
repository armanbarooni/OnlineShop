using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;

namespace OnlineShop.Application.Features.User.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<Result<string>>
    {
        public Guid UserId { get; set; }
        public UpdateProfileDto Request { get; set; } = null!;
    }
}
