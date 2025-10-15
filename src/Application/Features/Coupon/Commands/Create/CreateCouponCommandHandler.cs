using AutoMapper;
using MediatR;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Coupon;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;

namespace OnlineShop.Application.Features.Coupon.Commands.Create
{
    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Result<Guid>>
    {
        private readonly ICouponRepository _couponRepository;
        private readonly IMapper _mapper;

        public CreateCouponCommandHandler(ICouponRepository couponRepository, IMapper mapper)
        {
            _couponRepository = couponRepository;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
        {
            if (request.CouponDto == null)
                return Result<Guid>.Failure("درخواست نمی‌تواند خالی باشد");

            // Check if coupon code already exists
            var exists = await _couponRepository.ExistsByCodeAsync(request.CouponDto.Code, cancellationToken);
            if (exists)
                return Result<Guid>.Failure("کوپنی با این کد قبلاً ثبت شده است");

            var coupon = _mapper.Map<Domain.Entities.Coupon>(request.CouponDto);
            await _couponRepository.AddAsync(coupon, cancellationToken);

            return Result<Guid>.Success(coupon.Id);
        }
    }
}
