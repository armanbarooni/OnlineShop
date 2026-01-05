using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Payment;

namespace OnlineShop.Application.Features.Payment.Commands.InitiatePayment
{
    public class InitiatePaymentCommand : IRequest<Result<PaymentInitiationResultDto>>
    {
        public Guid UserId { get; set; }
        public InitiatePaymentDto Request { get; set; } = null!;
    }
}
