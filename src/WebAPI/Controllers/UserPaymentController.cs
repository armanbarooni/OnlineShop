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
using OnlineShop.Application.Features.UserPayment.Commands.ProcessPayment;
using OnlineShop.Application.Features.UserPayment.Commands.VerifyPayment;

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

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting payment history for user: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", currentUserGuid, pageNumber, pageSize);
            
            var query = new GetUserPaymentsByUserIdQuery 
            { 
                UserId = currentUserGuid,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved payment history for user: {UserId}, Total: {TotalCount}", currentUserGuid, result.Data?.TotalCount ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve payment history for user: {UserId}. Error: {Error}", currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
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
                return Forbid();

            _logger.LogInformation("Getting user payments for user: {UserId}", userId);
            var query = new GetUserPaymentsByUserIdQuery 
            { 
                UserId = userId,
                PageNumber = 1,
                PageSize = 100 // Default for backward compatibility
            };
            var result = await _mediator.Send(query, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved payments for user: {UserId}", userId);
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
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userGuid))
            {
                _logger.LogWarning("Create payment unauthorized: NameIdentifier claim missing or invalid. RawClaimValue='{RawUserId}'", userIdClaim ?? "<null>");
                return Unauthorized("User not authenticated");
            }

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

        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest? request, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Verify payment belongs to user
            var paymentResult = await _mediator.Send(new GetUserPaymentByIdQuery { Id = id }, cancellationToken);
            if (!paymentResult.IsSuccess)
                return NotFound(paymentResult);

            if (!User.IsInRole("Admin") && paymentResult.Data?.UserId != currentUserGuid)
                return Forbid();

            _logger.LogInformation("Processing payment: {PaymentId} by user: {UserId}", id, currentUserGuid);

            var command = new ProcessPaymentCommand
            {
                PaymentId = id,
                GatewayTransactionId = request?.GatewayTransactionId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Payment processed successfully: {PaymentId} by user: {UserId}", id, currentUserGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to process payment: {PaymentId} by user: {UserId}. Error: {Error}", id, currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("{id}/verify")]
        public async Task<IActionResult> VerifyPayment(Guid id, [FromBody] VerifyPaymentRequest? request, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Verify payment belongs to user
            var paymentResult = await _mediator.Send(new GetUserPaymentByIdQuery { Id = id }, cancellationToken);
            if (!paymentResult.IsSuccess)
                return NotFound(paymentResult);

            if (!User.IsInRole("Admin") && paymentResult.Data?.UserId != currentUserGuid)
                return Forbid();

            _logger.LogInformation("Verifying payment: {PaymentId} by user: {UserId}", id, currentUserGuid);

            var command = new VerifyPaymentCommand
            {
                PaymentId = id,
                TransactionId = request?.TransactionId,
                GatewayResponse = request?.GatewayResponse
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Payment verified successfully: {PaymentId} by user: {UserId}", id, currentUserGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to verify payment: {PaymentId} by user: {UserId}. Error: {Error}", id, currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        public class ProcessPaymentRequest
        {
            public string? GatewayTransactionId { get; set; }
        }

        public class VerifyPaymentRequest
        {
            public string? TransactionId { get; set; }
            public string? GatewayResponse { get; set; }
        }
    }
}
