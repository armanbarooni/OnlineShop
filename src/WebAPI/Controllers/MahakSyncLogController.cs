using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.MahakSyncLog;
using OnlineShop.Application.Features.MahakSyncLog.Command.Create;
using OnlineShop.Application.Features.MahakSyncLog.Command.Delete;
using OnlineShop.Application.Features.MahakSyncLog.Command.Update;
using OnlineShop.Application.Features.MahakSyncLog.Queries.GetAll;
using OnlineShop.Application.Features.MahakSyncLog.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MahakSyncLogController(IMediator mediator, ILogger<MahakSyncLogController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<MahakSyncLogController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all Mahak sync logs");
            var result = await _mediator.Send(new GetAllMahakSyncLogsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} Mahak sync logs", result.Data?.Count ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve Mahak sync logs: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting Mahak sync log by ID: {Id}", id);
            var result = await _mediator.Send(new GetMahakSyncLogByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved Mahak sync log: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Mahak sync log not found: {Id}", id);
            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMahakSyncLogDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating Mahak sync log by user: {UserId}", userId);
            
            var command = new CreateMahakSyncLogCommand { MahakSyncLog = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak sync log created successfully: {Id}", result.Data?.Id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create Mahak sync log. Error: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMahakSyncLogDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating Mahak sync log: {Id} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateMahakSyncLogCommand { MahakSyncLog = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak sync log updated successfully: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update Mahak sync log: {Id}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting Mahak sync log: {Id} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteMahakSyncLogCommand { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak sync log deleted successfully: {Id}", id);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete Mahak sync log: {Id}. Error: {Error}", id, result.ErrorMessage);
            return NotFound(result);
        }
    }
}

