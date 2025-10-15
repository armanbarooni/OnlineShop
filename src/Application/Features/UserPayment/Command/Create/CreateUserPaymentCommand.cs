using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Command.Create
{
    public class CreateUserPaymentCommand : IRequest<Result<UserPaymentDto>>
    {
        public CreateUserPaymentDto UserPayment { get; set; } = null!;
    }
}

