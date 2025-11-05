using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OnlineShop.Application.Features.Coupon.Commands.Create;
using OnlineShop.Application.Features.Coupon.Commands.ApplyCoupon;
using OnlineShop.Application.Features.Coupon.Queries.ValidateCoupon;
using OnlineShop.Application.Features.Coupon.Queries.GetAll;
using OnlineShop.Application.DTOs.Coupon;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CouponController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all coupons (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCoupons()
        {
            var query = new GetAllCouponsQuery();
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Create a new coupon (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto couponDto)
        {
            var command = new CreateCouponCommand { CouponDto = couponDto };
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Validate a coupon code
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
        {
            var query = new ValidateCouponQuery
            {
                Code = request.Code,
                UserId = request.UserId,
                OrderTotal = request.OrderTotal
            };
            
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
                return Ok(result.Data);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Apply a coupon to an order
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            var command = new ApplyCouponCommand
            {
                Code = request.Code,
                UserId = request.UserId,
                OrderId = request.OrderId,
                OrderTotal = request.OrderTotal
            };
            
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return Ok(result.Data);
            
            return BadRequest(result);
        }
    }

    public class ValidateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }

    public class ApplyCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
