using MediatR;
using OnlineShop.Application.Common.Models;

namespace OnlineShop.Application.Features.UserPayment.Command.Delete
{
    public class DeleteUserPaymentCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
    }
}

