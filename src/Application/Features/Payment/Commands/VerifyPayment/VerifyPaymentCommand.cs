using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Payment;

namespace OnlineShop.Application.Features.Payment.Commands.VerifyPayment
{
    public class VerifyPaymentCommand : IRequest<Result<PaymentVerificationResultDto>>
    {
        public VerifyPaymentDto Request { get; set; } = null!;
    }
}
