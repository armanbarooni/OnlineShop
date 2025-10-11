using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;
using OnlineShop.Application.Features.Wishlist.Command.Create;
using OnlineShop.Application.Features.Wishlist.Command.Delete;
using OnlineShop.Application.Features.Wishlist.Queries.GetByUserId;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WishlistController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Result<IEnumerable<WishlistDto>>>> GetByUserId(Guid userId)
        {
            var result = await _mediator.Send(new GetWishlistByUserIdQuery { UserId = userId });
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Result<WishlistDto>>> AddToWishlist([FromBody] CreateWishlistDto wishlist)
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            var result = await _mediator.Send(new CreateWishlistCommand 
            { 
                Wishlist = wishlist,
                UserId = userGuid
            });
            
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetByUserId), new { userId = userGuid }, result);
        }

        [HttpDelete("user/{userId}/product/{productId}")]
        public async Task<ActionResult<Result<bool>>> RemoveFromWishlist(Guid userId, Guid productId)
        {
            var result = await _mediator.Send(new DeleteWishlistCommand 
            { 
                UserId = userId,
                ProductId = productId
            });
            
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
    }
}
