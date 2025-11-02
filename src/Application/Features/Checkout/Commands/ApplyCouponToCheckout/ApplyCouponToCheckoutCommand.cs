using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.ApplyCouponToCheckout
{
    public class ApplyCouponToCheckoutCommand : IRequest<Result<ApplyCouponToCheckoutResultDto>>
    {
        public Guid UserId { get; set; }
        public ApplyCouponToCheckoutRequestDto Request { get; set; } = null!;
    }
}

