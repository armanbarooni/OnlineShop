using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Features.StockAlert.Commands.CreateStockAlert;
using OnlineShop.Application.Features.StockAlert.Commands.ProcessStockAlerts;
using OnlineShop.Application.Features.StockAlert.Queries.GetStockAlerts;
using OnlineShop.Application.DTOs.StockAlert;
using System.Security.Claims;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockAlertController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockAlertController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockAlert([FromBody] CreateStockAlertRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not authenticated.");

            var command = new CreateStockAlertCommand
            {
                ProductId = request.ProductId,
                ProductVariantId = request.ProductVariantId,
                UserId = userId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                NotificationMethod = request.NotificationMethod
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetStockAlerts(
            [FromQuery] string? userId = null,
            [FromQuery] Guid? productId = null,
            [FromQuery] bool? notified = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetStockAlertsQuery
            {
                UserId = userId,
                ProductId = productId,
                Notified = notified,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result);
        }

        [HttpPost("process")]
        [Authorize(Roles = "Admin")] // Only admins can process stock alerts
        public async Task<IActionResult> ProcessStockAlerts(
            [FromBody] ProcessStockAlertsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(new { ProcessedCount = result.Data, Message = "هشدارهای موجودی با موفقیت پردازش شدند" });

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAlert(Guid id, CancellationToken cancellationToken)
        {
            // TODO: Implement delete stock alert command
            return Ok(new { Message = "حذف هشدار موجودی پیاده‌سازی خواهد شد" });
        }
    }
}
