using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;

namespace OnlineShop.Application.Features.Coupon.Queries.GetAll
{
    public class GetAllCouponsQuery : IRequest<Result<IEnumerable<CouponDto>>>
    {
    }
}




