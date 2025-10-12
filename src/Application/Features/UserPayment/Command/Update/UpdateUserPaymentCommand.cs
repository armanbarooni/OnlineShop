using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Command.Update
{
    public class UpdateUserPaymentCommand : IRequest<Result<UserPaymentDto>>
    {
        public UpdateUserPaymentDto UserPayment { get; set; } = null!;
    }
}
