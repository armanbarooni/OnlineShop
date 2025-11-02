using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetDefaultAddress
{
    public class GetDefaultUserAddressQuery : IRequest<Result<UserAddressDto>>
    {
        public Guid UserId { get; set; }
    }
}

