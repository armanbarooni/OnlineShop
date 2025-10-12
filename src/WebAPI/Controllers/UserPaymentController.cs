using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserPayment;
using OnlineShop.Application.Features.UserPayment.Command.Create;
using OnlineShop.Application.Features.UserPayment.Command.Update;
using OnlineShop.Application.Features.UserPayment.Command.Delete;
using OnlineShop.Application.Features.UserPayment.Queries.GetById;
using OnlineShop.Application.Features.UserPayment.Queries.GetByUserId;
using OnlineShop.Application.Features.UserPayment.Queries.GetByOrderId;
using OnlineShop.Application.Features.UserPayment.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserPaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserPaymentController> _logger;

        public UserPaymentController(IMediator mediator, ILogger<UserPaymentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all user payments");
            var result = await _mediator.Send(new GetAllUserPaymentsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} user payments", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve user payments: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user payment by ID: {PaymentId}", id);
            var result = await _mediator.Send(new GetUserPaymentByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved user payment: {PaymentId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("User payment not found: {PaymentId}", id);
            return NotFound(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if user can access this data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Users can only see their own payments unless they're admin
            if (currentUserGuid != userId && !User.IsInRole("Admin"))
                return Forbid("Access denied");

            _logger.LogInformation("Getting user payments for user: {UserId}", userId);
            var result = await _mediator.Send(new GetUserPaymentsByUserIdQuery { UserId = userId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} payments for user: {UserId}", result.Data?.Count() ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve payments for user: {UserId}. Error: {Error}", userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user payments for order: {OrderId}", orderId);
            var result = await _mediator.Send(new GetUserPaymentsByOrderIdQuery { OrderId = orderId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} payments for order: {OrderId}", result.Data?.Count() ?? 0, orderId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve payments for order: {OrderId}. Error: {Error}", orderId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserPaymentDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            dto.UserId = userGuid;
            _logger.LogInformation("Creating user payment for user: {UserId}", userGuid);
            
            var command = new CreateUserPaymentCommand { UserPayment = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User payment created successfully: {PaymentId} for user: {UserId}", result.Data?.Id, userGuid);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create user payment for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserPaymentDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            dto.Id = id;
            _logger.LogInformation("Updating user payment: {PaymentId} by user: {UserId}", id, userId);
            
            var command = new UpdateUserPaymentCommand { UserPayment = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User payment updated successfully: {PaymentId} by user: {UserId}", id, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update user payment: {PaymentId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting user payment: {PaymentId} by user: {UserId}", id, userId);
            
            var command = new DeleteUserPaymentCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User payment deleted successfully: {PaymentId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete user payment: {PaymentId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return NotFound(result);
        }
    }
}
