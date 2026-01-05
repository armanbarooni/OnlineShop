using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;

namespace OnlineShop.Application.Features.User.Commands.CreateAddress
{
    public class CreateAddressCommand : IRequest<Result<Guid>>
    {
        public Guid UserId { get; set; }
        public CreateAddressDto Request { get; set; } = null!;
    }
}
