using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;

namespace OnlineShop.Application.Features.UserPayment.Commands.VerifyPayment
{
    public class VerifyPaymentCommand : IRequest<Result<UserPaymentDto>>
    {
        public Guid PaymentId { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
    }
}

