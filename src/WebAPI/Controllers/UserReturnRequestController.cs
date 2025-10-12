using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserReturnRequest;
using OnlineShop.Application.Features.UserReturnRequest.Command.Create;
using OnlineShop.Application.Features.UserReturnRequest.Command.Update;
using OnlineShop.Application.Features.UserReturnRequest.Command.Delete;
using OnlineShop.Application.Features.UserReturnRequest.Command.Approve;
using OnlineShop.Application.Features.UserReturnRequest.Command.Reject;
using OnlineShop.Application.Features.UserReturnRequest.Queries.GetById;
using OnlineShop.Application.Features.UserReturnRequest.Queries.GetByUserId;
using OnlineShop.Application.Features.UserReturnRequest.Queries.GetPendingRequests;
using OnlineShop.Application.Features.UserReturnRequest.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserReturnRequestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserReturnRequestController> _logger;

        public UserReturnRequestController(IMediator mediator, ILogger<UserReturnRequestController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all user return requests");
            var result = await _mediator.Send(new GetAllUserReturnRequestsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} user return requests", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve user return requests: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequests(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting pending user return requests");
            var result = await _mediator.Send(new GetPendingUserReturnRequestsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} pending return requests", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve pending return requests: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user return request by ID: {RequestId}", id);
            var result = await _mediator.Send(new GetUserReturnRequestByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved user return request: {RequestId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("User return request not found: {RequestId}", id);
            return NotFound(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if user can access this data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Users can only see their own return requests unless they're admin
            if (currentUserGuid != userId && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            _logger.LogInformation("Getting user return requests for user: {UserId}", userId);
            var result = await _mediator.Send(new GetUserReturnRequestsByUserIdQuery { UserId = userId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} return requests for user: {UserId}", result.Data?.Count() ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve return requests for user: {UserId}. Error: {Error}", userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserReturnRequestDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            dto.UserId = userGuid;
            _logger.LogInformation("Creating user return request for user: {UserId}", userGuid);
            
            var command = new CreateUserReturnRequestCommand { UserReturnRequest = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User return request created successfully: {RequestId} for user: {UserId}", result.Data?.Id, userGuid);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create user return request for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserReturnRequestDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            // Users can only update their own return requests unless they're admin
            if (dto.UserId != userGuid && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            dto.Id = id;
            _logger.LogInformation("Updating user return request: {RequestId} by user: {UserId}", id, userGuid);
            
            var command = new UpdateUserReturnRequestCommand { UserReturnRequest = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User return request updated successfully: {RequestId} by user: {UserId}", id, userGuid);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update user return request: {RequestId} by user: {UserId}. Error: {Error}", id, userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveUserReturnRequestDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Approving user return request: {RequestId} by user: {UserId}", id, userId);
            
            var command = new ApproveUserReturnRequestCommand 
            { 
                Id = id,
                AdminNotes = dto.AdminNotes
            };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User return request approved successfully: {RequestId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to approve user return request: {RequestId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectUserReturnRequestDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Rejecting user return request: {RequestId} by user: {UserId}", id, userId);
            
            var command = new RejectUserReturnRequestCommand 
            { 
                Id = id,
                RejectionReason = dto.RejectionReason,
                AdminNotes = dto.AdminNotes
            };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User return request rejected successfully: {RequestId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to reject user return request: {RequestId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting user return request: {RequestId} by user: {UserId}", id, userId);
            
            var command = new DeleteUserReturnRequestCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User return request deleted successfully: {RequestId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete user return request: {RequestId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return NotFound(result);
        }
    }
}
