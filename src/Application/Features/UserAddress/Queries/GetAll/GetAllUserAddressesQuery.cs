using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetAll
{
    public class GetAllUserAddressesQuery : IRequest<Result<List<UserAddressDto>>>
    {
    }
}


