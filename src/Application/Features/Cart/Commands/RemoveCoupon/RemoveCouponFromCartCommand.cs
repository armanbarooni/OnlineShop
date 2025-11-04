using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;

namespace OnlineShop.Application.Features.Cart.Commands.RemoveCoupon
{
    public class RemoveCouponFromCartCommand : IRequest<Result<RemoveCouponFromCartResultDto>>
    {
        public Guid UserId { get; set; }
        public Guid? CartId { get; set; }
    }
}

