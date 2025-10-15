using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetByUserId
{
    public class GetUserAddressesByUserIdQuery : IRequest<Result<IEnumerable<UserAddressDto>>>
    {
        public Guid UserId { get; set; }
    }
}

