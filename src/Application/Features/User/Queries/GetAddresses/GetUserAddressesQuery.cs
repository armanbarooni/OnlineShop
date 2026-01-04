using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.User;

namespace OnlineShop.Application.Features.User.Queries.GetAddresses
{
    public class GetUserAddressesQuery : IRequest<Result<List<UserAddressDto>>>
    {
        public Guid UserId { get; set; }
    }
}
