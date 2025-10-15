using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;

namespace OnlineShop.Application.Features.Coupon.Commands.Create
{
    public class CreateCouponCommand : IRequest<Result<Guid>>
    {
        public CreateCouponDto CouponDto { get; set; } = null!;
    }
}
