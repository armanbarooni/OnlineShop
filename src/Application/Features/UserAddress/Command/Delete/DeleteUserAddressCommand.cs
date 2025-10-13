using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserAddress.Command.Delete
{
    public class DeleteUserAddressCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

