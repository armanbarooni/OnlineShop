using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserReturnRequest.Command.Delete
{
    public class DeleteUserReturnRequestCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

