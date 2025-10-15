using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommand : IRequest<Result<OtpResponseDto>>
    {
        public SendOtpDto Request { get; set; } = new();
    }
}


