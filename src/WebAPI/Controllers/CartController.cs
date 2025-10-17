using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Cart;
using OnlineShop.Application.Features.Cart.Command.AddItem;
using OnlineShop.Application.Features.Cart.Command.Create;
using OnlineShop.Application.Features.Cart.Command.Update;
using OnlineShop.Application.Features.Cart.Command.Delete;
using OnlineShop.Application.Features.Cart.Queries.GetByUserId;
using OnlineShop.Application.Features.Cart.Queries.GetAll;
using OnlineShop.Application.Features.Cart.Queries.GetById;

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<IEnumerable<CartDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllCartsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<CartDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCartByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);
            return Ok(result);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<CartDto>>> UpdateCart(Guid id, [FromBody] UpdateCartDto cart)
        {
            if (id != cart.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateCartCommand { Cart = cart });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCart(Guid id)
        {
            var result = await _mediator.Send(new DeleteCartCommand { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }

        [HttpPost("items")]
        [HttpPost("add")] // Alternative route for backward compatibility with tests
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
