using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Auth;

namespace OnlineShop.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommand : IRequest<Result<OtpResponseDto>>
    {
        public VerifyOtpDto Request { get; set; } = new();
    }
}


