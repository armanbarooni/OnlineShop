using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserProfile.Command.Delete
{
    public class DeleteUserProfileCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}


