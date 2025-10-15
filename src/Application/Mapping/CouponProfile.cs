using AutoMapper;
using OnlineShop.Application.DTOs.Coupon;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Application.Mapping
{
    public class CouponProfile : Profile
    {
        public CouponProfile()
        {
            CreateMap<Coupon, CouponDto>();
            CreateMap<CreateCouponDto, Coupon>();
            CreateMap<UpdateCouponDto, Coupon>();
        }
    }
}
