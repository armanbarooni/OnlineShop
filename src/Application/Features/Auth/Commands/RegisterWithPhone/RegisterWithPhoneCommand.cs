using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Features.Auth.Commands.RegisterWithPhone
{
    public class RegisterWithPhoneCommand : IRequest<Result<AuthResponseDto>>
    {
        public RegisterWithPhoneDto Request { get; set; } = new();
    }
}

