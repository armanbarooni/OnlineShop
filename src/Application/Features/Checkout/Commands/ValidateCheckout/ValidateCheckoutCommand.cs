using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.ValidateCheckout
{
    public class ValidateCheckoutCommand : IRequest<Result<CheckoutValidationResultDto>>
    {
        public Guid UserId { get; set; }
        public CheckoutRequestDto Request { get; set; } = null!;
    }
}

