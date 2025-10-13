using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.SyncErrorLog;
using OnlineShop.Application.Features.SyncErrorLog.Command.Create;
using OnlineShop.Application.Features.SyncErrorLog.Command.Delete;
using OnlineShop.Application.Features.SyncErrorLog.Command.Update;
using OnlineShop.Application.Features.SyncErrorLog.Queries.GetAll;
using OnlineShop.Application.Features.SyncErrorLog.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SyncErrorLogController(IMediator mediator, ILogger<SyncErrorLogController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<SyncErrorLogController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all sync error logs");
            var result = await _mediator.Send(new GetAllSyncErrorLogsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} sync error logs", result.Data?.Count ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve sync error logs: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting sync error log by ID: {Id}", id);
            var result = await _mediator.Send(new GetSyncErrorLogByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved sync error log: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Sync error log not found: {Id}", id);
            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSyncErrorLogDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating sync error log by user: {UserId}", userId);
            
            var command = new CreateSyncErrorLogCommand { SyncErrorLog = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Sync error log created successfully: {Id}", result.Data?.Id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create sync error log. Error: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSyncErrorLogDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating sync error log: {Id} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateSyncErrorLogCommand { SyncErrorLog = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Sync error log updated successfully: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update sync error log: {Id}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting sync error log: {Id} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteSyncErrorLogCommand { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Sync error log deleted successfully: {Id}", id);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete sync error log: {Id}. Error: {Error}", id, result.ErrorMessage);
            return NotFound(result);
        }
    }
}

