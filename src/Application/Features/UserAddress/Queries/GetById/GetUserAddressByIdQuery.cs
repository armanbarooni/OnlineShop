using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserAddress;

namespace OnlineShop.Application.Features.UserAddress.Queries.GetById
{
    public class GetUserAddressByIdQuery : IRequest<Result<UserAddressDto>>
    {
        public Guid Id { get; set; }
    }
}

