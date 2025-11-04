using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.SearchUserAddresses
{
    public class SearchUserAddressesQuery : IRequest<Result<IEnumerable<UserAddressDto>>>
    {
        public Guid UserId { get; set; }
        public string? SearchQuery { get; set; }
    }
}

