using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Features.Cart.Command.AddItem;
using OnlineShop.Application.Features.Cart.Command.Create;
using OnlineShop.Application.Features.Cart.Queries.GetByUserId;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Result<CartDto?>>> GetByUserId(Guid userId)
        {
            var result = await _mediator.Send(new GetCartByUserIdQuery { UserId = userId });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<CartDto>>> CreateCart([FromBody] CreateCartDto cart)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new CreateCartCommand 
            { 
                Cart = cart,
                UserId = userGuid
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByUserId), new { userId = userGuid }, result);
        }

        [HttpPost("items")]
        public async Task<ActionResult<Result<CartItemDto>>> AddItem([FromBody] CreateCartItemDto cartItem)
        {
            var result = await _mediator.Send(new AddCartItemCommand 
            { 
                CartItem = cartItem
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByUserId), new { userId = cartItem.CartId }, result);
        }
    }
}
