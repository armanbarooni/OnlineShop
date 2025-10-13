using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommand : IRequest<Result<CheckoutResultDto>>
    {
        public Guid UserId { get; set; }
        public CheckoutRequestDto Request { get; set; } = null!;
    }
}

