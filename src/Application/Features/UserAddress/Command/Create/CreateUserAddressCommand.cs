using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Command.Create
{
    public class CreateUserAddressCommand : IRequest<Result<UserAddressDto>>
    {
        public CreateUserAddressDto? UserAddress { get; set; }
        public Guid UserId { get; set; }
    }
}

