using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserAddress.Command.SetDefault
{
    public class SetDefaultUserAddressCommand : IRequest<Result<bool>>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
    }
}
