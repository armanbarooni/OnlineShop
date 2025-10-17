using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Coupon.Queries.GetAll
{
    public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, Result<IEnumerable<CouponDto>>>
    {
        private readonly ICouponRepository _couponRepository;

        public GetAllCouponsQueryHandler(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<Result<IEnumerable<CouponDto>>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
        {
            var coupons = await _couponRepository.GetAllAsync(cancellationToken);

            var couponDtos = coupons.Select(coupon => new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Name = coupon.Name,
                Description = coupon.Description,
                DiscountPercentage = coupon.DiscountPercentage,
                DiscountAmount = coupon.DiscountAmount,
                MinimumPurchase = coupon.MinimumPurchase,
                MaximumDiscount = coupon.MaximumDiscount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                IsActive = coupon.IsActive,
                IsSingleUse = coupon.IsSingleUse,
                ApplicableCategories = coupon.ApplicableCategories,
                ApplicableProducts = coupon.ApplicableProducts,
                ApplicableUsers = coupon.ApplicableUsers,
                CreatedAt = coupon.CreatedAt,
                UpdatedAt = coupon.UpdatedAt
            }).ToList();

            return Result<IEnumerable<CouponDto>>.Success(couponDtos);
        }
    }
}

