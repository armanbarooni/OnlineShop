using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.SavedCart;
using OnlineShop.Application.Features.SavedCart.Command.Create;
using OnlineShop.Application.Features.SavedCart.Command.Update;
using OnlineShop.Application.Features.SavedCart.Command.Delete;
using OnlineShop.Application.Features.SavedCart.Queries.GetById;
using OnlineShop.Application.Features.SavedCart.Queries.GetByUserId;
using OnlineShop.Application.Features.SavedCart.Queries.GetFavoriteCarts;
using OnlineShop.Application.Features.SavedCart.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavedCartController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SavedCartController> _logger;

        public SavedCartController(IMediator mediator, ILogger<SavedCartController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all saved carts");
            var result = await _mediator.Send(new GetAllSavedCartsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} saved carts", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve saved carts: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting saved cart by ID: {SavedCartId}", id);
            var result = await _mediator.Send(new GetSavedCartByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved saved cart: {SavedCartId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Saved cart not found: {SavedCartId}", id);
            return NotFound(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if user can access this data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Users can only see their own saved carts unless they're admin
            if (currentUserGuid != userId && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            _logger.LogInformation("Getting saved carts for user: {UserId}", userId);
            var result = await _mediator.Send(new GetSavedCartsByUserIdQuery { UserId = userId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} saved carts for user: {UserId}", result.Data?.Count() ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve saved carts for user: {UserId}. Error: {Error}", userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("user/{userId}/favorites")]
        public async Task<IActionResult> GetFavoriteCarts(Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if user can access this data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Users can only see their own favorite carts unless they're admin
            if (currentUserGuid != userId && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            _logger.LogInformation("Getting favorite saved carts for user: {UserId}", userId);
            var result = await _mediator.Send(new GetFavoriteSavedCartsQuery { UserId = userId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} favorite saved carts for user: {UserId}", result.Data?.Count() ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve favorite saved carts for user: {UserId}. Error: {Error}", userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSavedCartDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            dto.UserId = userGuid;
            _logger.LogInformation("Creating saved cart for user: {UserId}", userGuid);
            
            var command = new CreateSavedCartCommand { SavedCart = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Saved cart created successfully: {SavedCartId} for user: {UserId}", result.Data?.Id, userGuid);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create saved cart for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSavedCartDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            // Users can only update their own saved carts unless they're admin
            if (dto.UserId != userGuid && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            dto.Id = id;
            _logger.LogInformation("Updating saved cart: {SavedCartId} by user: {UserId}", id, userGuid);
            
            var command = new UpdateSavedCartCommand { SavedCart = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Saved cart updated successfully: {SavedCartId} by user: {UserId}", id, userGuid);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update saved cart: {SavedCartId} by user: {UserId}. Error: {Error}", id, userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Deleting saved cart: {SavedCartId} by user: {UserId}", id, userGuid);
            
            var command = new DeleteSavedCartCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Saved cart deleted successfully: {SavedCartId} by user: {UserId}", id, userGuid);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete saved cart: {SavedCartId} by user: {UserId}. Error: {Error}", id, userGuid, result.ErrorMessage);
            return NotFound(result);
        }
    }
}
