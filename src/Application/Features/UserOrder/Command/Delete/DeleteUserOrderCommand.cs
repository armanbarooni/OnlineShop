using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserOrder.Command.Delete
{
    public class DeleteUserOrderCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

