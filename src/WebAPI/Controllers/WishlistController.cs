using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.Wishlist;
using OnlineShop.Application.Features.Wishlist.Command.Create;
using OnlineShop.Application.Features.Wishlist.Command.Delete;
using OnlineShop.Application.Features.Wishlist.Command.Update;
using OnlineShop.Application.Features.Wishlist.Queries.GetByUserId;
using OnlineShop.Application.Features.Wishlist.Queries.GetAll;
using OnlineShop.Application.Features.Wishlist.Queries.GetById;
using OnlineShop.Domain.Entities;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(IMediator mediator, UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<IEnumerable<WishlistDto>>>> GetAll()
        {
            var result = await _mediator.Send(new GetAllWishlistsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<WishlistDto>>> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetWishlistByIdQuery { Id = id });
            if (!result.IsSuccess)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Result<IEnumerable<WishlistDto>>>> GetByUserId([FromRoute(Name = "userId")] string userIdentifier)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return BadRequest(Result<IEnumerable<WishlistDto>>.Failure("User identifier is required"));
            }

            Guid userId;
            if (!Guid.TryParse(userIdentifier, out userId))
            {
                var user = await _userManager.FindByEmailAsync(userIdentifier);
                if (user == null)
                {
                    return NotFound(Result<IEnumerable<WishlistDto>>.Failure("User not found"));
                }

                userId = user.Id;
            }

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

        [HttpPut("{id}")]
        public async Task<ActionResult<Result<WishlistDto>>> UpdateWishlist(Guid id, [FromBody] UpdateWishlistDto wishlist)
        {
            if (id != wishlist.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(new UpdateWishlistCommand { Wishlist = wishlist });
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Result<bool>>> RemoveFromWishlistById(Guid id)
        {
            // Get the wishlist by ID first to get userId and productId
            var wishlistResult = await _mediator.Send(new GetWishlistByIdQuery { Id = id });
            if (!wishlistResult.IsSuccess)
                return NotFound(wishlistResult);

            var wishlist = wishlistResult.Data;
            if (wishlist == null)
                return NotFound(Result<bool>.Failure("Wishlist item not found"));

            var result = await _mediator.Send(new DeleteWishlistCommand 
            { 
                UserId = wishlist.UserId,
                ProductId = wishlist.ProductId
            });
            
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
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
