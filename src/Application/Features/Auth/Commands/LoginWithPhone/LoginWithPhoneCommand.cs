using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Features.Auth.Commands.LoginWithPhone
{
    public class LoginWithPhoneCommand : IRequest<Result<AuthResponseDto>>
    {
        public LoginWithPhoneDto Request { get; set; } = new();
    }
}

