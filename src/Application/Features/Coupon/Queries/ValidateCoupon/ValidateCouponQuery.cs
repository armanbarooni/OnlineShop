using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;

namespace OnlineShop.Application.Features.Coupon.Queries.ValidateCoupon
{
    public class ValidateCouponQuery : IRequest<Result<CouponValidationResultDto>>
    {
        public string Code { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }
}
