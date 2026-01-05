using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.User.Commands.DeleteAddress
{
    public class DeleteAddressCommand : IRequest<Result<string>>
    {
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
    }
}
