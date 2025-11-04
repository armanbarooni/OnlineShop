using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Checkout;
using OnlineShop.Application.Features.Checkout.Commands.ProcessCheckout;
using OnlineShop.Application.Features.Checkout.Queries.GetCheckoutSummary;
using OnlineShop.Application.Features.Checkout.Commands.ValidateCheckout;
using OnlineShop.Application.Features.UserAddress.Queries.GetByUserId;
using OnlineShop.Application.Features.Checkout.Queries.GetPaymentMethods;
using OnlineShop.Application.Features.Checkout.Queries.GetShippingMethods;
using OnlineShop.Application.Features.Checkout.Commands.CalculateShipping;
using OnlineShop.Application.Features.Checkout.Commands.ApplyCouponToCheckout;
using OnlineShop.Application.Features.Checkout.Commands.RemoveCouponFromCheckout;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(IMediator mediator, ILogger<CheckoutController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Processing checkout for user: {UserId}, Cart: {CartId}", userGuid, request.CartId);

            var command = new ProcessCheckoutCommand
            {
                UserId = userGuid,
                Request = request
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Checkout successful for user: {UserId}, Order: {OrderId}", 
                    userGuid, result.Data?.Order.Id);
                return Ok(result);
            }

            _logger.LogWarning("Checkout failed for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting addresses for checkout for user: {UserId}", userGuid);

            var query = new GetUserAddressesByUserIdQuery { UserId = userGuid };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} addresses for user: {UserId}", result.Data?.Count() ?? 0, userGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve addresses for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("order-summary")]
        public async Task<IActionResult> GetOrderSummary([FromQuery] Guid? cartId = null, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting checkout summary for user: {UserId}, Cart: {CartId}", userGuid, cartId);

            var query = new GetCheckoutSummaryQuery { UserId = userGuid, CartId = cartId };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved checkout summary for user: {UserId}", userGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve checkout summary for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCheckout([FromBody] CheckoutRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Validating checkout for user: {UserId}, Cart: {CartId}", userGuid, request.CartId);

            var command = new ValidateCheckoutCommand
            {
                UserId = userGuid,
                Request = request
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Checkout validation completed for user: {UserId}. IsValid: {IsValid}", userGuid, result.Data?.IsValid);
                return Ok(result);
            }

            _logger.LogWarning("Checkout validation failed for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting available payment methods");

            var query = new GetAvailablePaymentMethodsQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} payment methods", result.Data?.Count() ?? 0);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve payment methods. Error: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("shipping-methods")]
        public async Task<IActionResult> GetShippingMethods(
            [FromQuery] Guid? addressId = null,
            [FromQuery] decimal? orderWeight = null,
            [FromQuery] decimal? orderValue = null,
            CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting shipping methods for user: {UserId}, Address: {AddressId}", userGuid, addressId);

            // If address provided, verify it belongs to user
            if (addressId.HasValue)
            {
                var addressQuery = new GetUserAddressesByUserIdQuery { UserId = userGuid };
                var addressesResult = await _mediator.Send(addressQuery, cancellationToken);
                if (!addressesResult.IsSuccess || !addressesResult.Data?.Any(a => a.Id == addressId.Value) == true)
                    return BadRequest("آدرس یافت نشد یا دسترسی ندارید");
            }

            var query = new GetShippingMethodsQuery
            {
                AddressId = addressId,
                OrderWeight = orderWeight,
                OrderValue = orderValue
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} shipping methods for user: {UserId}", result.Data?.Count() ?? 0, userGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve shipping methods for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("calculate-shipping")]
        public async Task<IActionResult> CalculateShipping([FromBody] CalculateShippingRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Calculating shipping for user: {UserId}, Method: {MethodId}, Address: {AddressId}", 
                userGuid, request.ShippingMethodId, request.AddressId);

            var command = new CalculateShippingCommand
            {
                UserId = userGuid,
                Request = request
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Shipping calculated successfully for user: {UserId}, Cost: {Cost}", userGuid, result.Data?.Cost);
                return Ok(result);
            }

            _logger.LogWarning("Failed to calculate shipping for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("apply-coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponToCheckoutRequestDto request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Applying coupon to checkout for user: {UserId}, Coupon: {CouponCode}, Cart: {CartId}", 
                userGuid, request.CouponCode, request.CartId);

            var command = new ApplyCouponToCheckoutCommand
            {
                UserId = userGuid,
                Request = request
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Coupon applied successfully for user: {UserId}, Discount: {Discount}", userGuid, result.Data?.DiscountAmount);
                return Ok(result);
            }

            _logger.LogWarning("Failed to apply coupon for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("remove-coupon")]
        public async Task<IActionResult> RemoveCoupon([FromQuery] Guid? cartId = null, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Removing coupon from checkout for user: {UserId}, Cart: {CartId}", userGuid, cartId);

            var command = new RemoveCouponFromCheckoutCommand
            {
                UserId = userGuid,
                CartId = cartId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Coupon removed successfully for user: {UserId}", userGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to remove coupon for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }
    }
}

