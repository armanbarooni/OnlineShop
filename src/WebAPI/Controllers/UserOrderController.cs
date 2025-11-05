using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.DTOs.UserOrder;
using OnlineShop.Application.Features.UserOrder.Command.Create;
using OnlineShop.Application.Features.UserOrder.Command.Update;
using OnlineShop.Application.Features.UserOrder.Command.Delete;
using OnlineShop.Application.Features.UserOrder.Queries.GetById;
using OnlineShop.Application.Features.UserOrder.Queries.GetByUserId;
using OnlineShop.Application.Features.UserOrder.Queries.GetAll;
using OnlineShop.Application.Features.UserOrder.Queries.Search;
using OnlineShop.Application.Features.UserOrder.Queries.GetOrderItems;
using OnlineShop.Application.Features.UserOrder.Queries.GetOrderTimeline;
using OnlineShop.Application.Features.UserOrder.Commands.CancelOrder;
using OnlineShop.Application.Features.UserOrder.Queries.GenerateInvoice;
using OnlineShop.Application.Features.UserOrder.Queries.GetRecentOrders;
using OnlineShop.Application.Features.UserOrder.Queries.GetOrderStatistics;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserOrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserOrderController> _logger;

        public UserOrderController(IMediator mediator, ILogger<UserOrderController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all user orders");
            var result = await _mediator.Send(new GetAllUserOrdersQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} user orders", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve user orders: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] UserOrderSearchCriteriaDto? criteria, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // If not admin, force criteria to only show user's own orders
            if (!User.IsInRole("Admin"))
            {
                if (criteria == null)
                    criteria = new UserOrderSearchCriteriaDto();
                criteria.UserId = currentUserGuid;
            }

            _logger.LogInformation("Searching user orders with criteria");
            var result = await _mediator.Send(new UserOrderSearchQuery { Criteria = criteria }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} user orders from search", result.Data?.TotalCount ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to search user orders: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user order by ID: {OrderId}", id);
            var result = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved user order: {OrderId}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("User order not found: {OrderId}", id);
            return NotFound(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if user can access this data
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // Users can only see their own orders unless they're admin
            if (currentUserGuid != userId && !User.IsInRole("Admin"))
                return Forbid();

            _logger.LogInformation("Getting user orders for user: {UserId}", userId);
            var result = await _mediator.Send(new GetUserOrdersByUserIdQuery { UserId = userId }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} orders for user: {UserId}", result.Data?.Count() ?? 0, userId);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve orders for user: {UserId}. Error: {Error}", userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserOrderDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Creating user order for user: {UserId}", userGuid);
            
            var command = new CreateUserOrderCommand { UserOrder = dto, UserId = userGuid };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User order created successfully: {OrderId} for user: {UserId}", result.Data?.Id, userGuid);
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            }
            
            _logger.LogWarning("Failed to create user order for user: {UserId}. Error: {Error}", userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserOrderDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User not authenticated");

            dto.Id = id;
            _logger.LogInformation("Updating user order: {OrderId} by user: {UserId}", id, userGuid);
            
            var command = new UpdateUserOrderCommand { UserOrder = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User order updated successfully: {OrderId} by user: {UserId}", id, userGuid);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update user order: {OrderId} by user: {UserId}. Error: {Error}", id, userGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting user order: {OrderId} by user: {UserId}", id, userId);
            
            var command = new DeleteUserOrderCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User order deleted successfully: {OrderId} by user: {UserId}", id, userId);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete user order: {OrderId} by user: {UserId}. Error: {Error}", id, userId, result.ErrorMessage);
            return NotFound(result);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetOrderItems(Guid id, CancellationToken cancellationToken = default)
        {
            // Check if user can access this order
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // First verify order belongs to user (unless admin)
            var orderResult = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            if (!orderResult.IsSuccess)
                return NotFound(orderResult);

            if (!User.IsInRole("Admin"))
            {
                if (orderResult.Data?.UserId != currentUserGuid)
                    return Forbid();
            }

            _logger.LogInformation("Getting order items for order: {OrderId}", id);
            var result = await _mediator.Send(new GetUserOrderItemsQuery { OrderId = id }, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} items for order: {OrderId}", result.Data?.Count() ?? 0, id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve order items for order: {OrderId}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}/track")]
        public async Task<IActionResult> TrackOrder(Guid id, CancellationToken cancellationToken = default)
        {
            // Check if user can access this order
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // First verify order belongs to user (unless admin)
            var orderResult = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            if (!orderResult.IsSuccess)
                return NotFound(orderResult);

            if (!User.IsInRole("Admin"))
            {
                if (orderResult.Data?.UserId != currentUserGuid)
                    return Forbid();
            }

            _logger.LogInformation("Tracking order: {OrderId}", id);
            var result = await _mediator.Send(new GetOrderTimelineQuery { OrderId = id }, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved tracking info for order: {OrderId}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve tracking info for order: {OrderId}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}/status-history")]
        public async Task<IActionResult> GetOrderStatusHistory(Guid id, CancellationToken cancellationToken = default)
        {
            // Check if user can access this order
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // First verify order belongs to user (unless admin)
            var orderResult = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            if (!orderResult.IsSuccess)
                return NotFound(orderResult);

            if (!User.IsInRole("Admin"))
            {
                if (orderResult.Data?.UserId != currentUserGuid)
                    return Forbid();
            }

            _logger.LogInformation("Getting status history for order: {OrderId}", id);
            var result = await _mediator.Send(new GetOrderTimelineQuery { OrderId = id }, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved status history for order: {OrderId}", id);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve status history for order: {OrderId}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting recent orders for user: {UserId}, Limit: {Limit}", currentUserGuid, limit);

            var query = new GetRecentUserOrdersQuery
            {
                UserId = currentUserGuid,
                Limit = limit > 0 && limit <= 20 ? limit : 5 // Max 20, default 5
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} recent orders for user: {UserId}", result.Data?.Count() ?? 0, currentUserGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve recent orders for user: {UserId}. Error: {Error}", currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetOrderStatistics(CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            _logger.LogInformation("Getting order statistics for user: {UserId}", currentUserGuid);

            var query = new GetUserOrderStatisticsQuery { UserId = currentUserGuid };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved order statistics for user: {UserId}", currentUserGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to retrieve order statistics for user: {UserId}. Error: {Error}", currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // First verify order belongs to user (unless admin)
            var orderResult = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            if (!orderResult.IsSuccess)
                return NotFound(orderResult);

            if (!User.IsInRole("Admin"))
            {
                if (orderResult.Data?.UserId != currentUserGuid)
                    return Forbid();
            }

            _logger.LogInformation("Cancelling order: {OrderId} by user: {UserId}", id, currentUserGuid);
            
            var command = new CancelOrderCommand
            {
                OrderId = id,
                CancellationReason = request?.Reason,
                UpdatedBy = currentUserId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Order cancelled successfully: {OrderId} by user: {UserId}", id, currentUserGuid);
                return Ok(result);
            }

            _logger.LogWarning("Failed to cancel order: {OrderId} by user: {UserId}. Error: {Error}", id, currentUserGuid, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(Guid id, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || !Guid.TryParse(currentUserId, out var currentUserGuid))
                return Unauthorized("User not authenticated");

            // First verify order belongs to user (unless admin)
            var orderResult = await _mediator.Send(new GetUserOrderByIdQuery { Id = id }, cancellationToken);
            if (!orderResult.IsSuccess)
                return NotFound(orderResult);

            if (!User.IsInRole("Admin"))
            {
                if (orderResult.Data?.UserId != currentUserGuid)
                    return Forbid();
            }

            _logger.LogInformation("Generating invoice for order: {OrderId}", id);

            var query = new GenerateOrderInvoiceQuery { OrderId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Invoice generated successfully for order: {OrderId}", id);
                // For now return HTML, can be changed to PDF later
                return File(result.Data!, "text/html", $"invoice-{id}.html");
            }

            _logger.LogWarning("Failed to generate invoice for order: {OrderId}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        public class CancelOrderRequest
        {
            public string? Reason { get; set; }
        }
    }
}
