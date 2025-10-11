using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Command.Update
{
    public class UpdateUserAddressCommand : IRequest<Result<UserAddressDto>>
    {
        public UpdateUserAddressDto? UserAddress { get; set; }
    }
}
