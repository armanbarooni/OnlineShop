using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Commands.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest<Result<UserPaymentDto>>
    {
        public Guid PaymentId { get; set; }
        public string? GatewayTransactionId { get; set; }
    }
}

