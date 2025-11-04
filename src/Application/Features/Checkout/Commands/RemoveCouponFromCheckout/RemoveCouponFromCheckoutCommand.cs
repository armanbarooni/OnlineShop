using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Checkout;

namespace OnlineShop.Application.Features.Checkout.Commands.RemoveCouponFromCheckout
{
    public class RemoveCouponFromCheckoutCommand : IRequest<Result<RemoveCouponFromCheckoutResultDto>>
    {
        public Guid UserId { get; set; }
        public Guid? CartId { get; set; }
    }
}

