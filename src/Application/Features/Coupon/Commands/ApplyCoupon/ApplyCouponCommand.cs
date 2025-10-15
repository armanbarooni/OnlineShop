using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;

namespace OnlineShop.Application.Features.Coupon.Commands.ApplyCoupon
{
    public class ApplyCouponCommand : IRequest<Result<ApplyCouponResultDto>>
    {
        public string Code { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
