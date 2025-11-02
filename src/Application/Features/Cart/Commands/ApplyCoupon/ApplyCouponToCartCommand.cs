using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Commands.ApplyCoupon
{
    public class ApplyCouponToCartCommand : IRequest<Result<ApplyCouponToCartResultDto>>
    {
        public Guid UserId { get; set; }
        public ApplyCouponToCartRequestDto Request { get; set; } = null!;
    }
}

