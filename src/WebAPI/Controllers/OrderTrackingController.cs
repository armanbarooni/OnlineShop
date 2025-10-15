using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OnlineShop.Application.Features.UserOrder.Commands.UpdateOrderStatus;
using OnlineShop.Application.Features.UserOrder.Commands.SetTrackingNumber;
using OnlineShop.Application.Features.UserOrder.Queries.GetOrderTimeline;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderTrackingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderTrackingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get order timeline with status history
        /// </summary>
        [HttpGet("{orderId}/timeline")]
        public async Task<IActionResult> GetOrderTimeline(Guid orderId)
        {
            var query = new GetOrderTimelineQuery { OrderId = orderId };
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
                return Ok(result.Data);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                Status = request.Status,
                Note = request.Note,
                UpdatedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            };
            
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Set tracking number (Admin only)
        /// </summary>
        [HttpPut("{orderId}/tracking")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetTrackingNumber(Guid orderId, [FromBody] SetTrackingNumberRequest request)
        {
            var command = new SetTrackingNumberCommand
            {
                OrderId = orderId,
                TrackingNumber = request.TrackingNumber,
                UpdatedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            };
            
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }
    }

    public class UpdateOrderStatusRequest
    {
        public OnlineShop.Domain.Enums.OrderStatus Status { get; set; }
        public string? Note { get; set; }
    }

    public class SetTrackingNumberRequest
    {
        public string TrackingNumber { get; set; } = string.Empty;
    }
}
