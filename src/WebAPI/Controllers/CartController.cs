using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Features.Cart.Commands.AddToCart;
using OnlineShop.Application.Features.Cart.Commands.RemoveFromCart;
using OnlineShop.Application.Features.Cart.Commands.UpdateCart;
using OnlineShop.Application.Features.Cart.Queries.GetCart;
using System.Security.Claims;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CartController> _logger;

        public CartController(IMediator mediator, ILogger<CartController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "کاربر یافت نشد" });
            }

            _logger.LogInformation("Adding item to cart for user {UserId}", userId);

            var command = new AddToCartCommand
            {
                UserId = userId,
                Item = dto
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "کاربر یافت نشد" });
            }

            var query = new GetCartQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "کاربر یافت نشد" });
            }

            var command = new UpdateCartCommand
            {
                UserId = userId,
                Item = dto
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("remove/{itemId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart([FromRoute] Guid itemId, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "کاربر یافت نشد" });
            }

            var command = new RemoveFromCartCommand
            {
                UserId = userId,
                CartItemId = itemId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
