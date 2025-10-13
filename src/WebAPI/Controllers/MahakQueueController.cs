using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.MahakQueue;
using OnlineShop.Application.Features.MahakQueue.Command.Create;
using OnlineShop.Application.Features.MahakQueue.Command.Delete;
using OnlineShop.Application.Features.MahakQueue.Command.Update;
using OnlineShop.Application.Features.MahakQueue.Queries.GetAll;
using OnlineShop.Application.Features.MahakQueue.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MahakQueueController(IMediator mediator, ILogger<MahakQueueController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<MahakQueueController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all Mahak queue items");
            var result = await _mediator.Send(new GetAllMahakQueuesQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} Mahak queue items", result.Data?.Count ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve Mahak queue items: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting Mahak queue item by ID: {Id}", id);
            var result = await _mediator.Send(new GetMahakQueueByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved Mahak queue item: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Mahak queue item not found: {Id}", id);
            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMahakQueueDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating Mahak queue item by user: {UserId}", userId);
            
            var command = new CreateMahakQueueCommand { MahakQueue = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak queue item created successfully: {Id}", result.Data?.Id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create Mahak queue item. Error: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMahakQueueDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating Mahak queue item: {Id} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateMahakQueueCommand { MahakQueue = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak queue item updated successfully: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update Mahak queue item: {Id}. Error: {Error}", id, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting Mahak queue item: {Id} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteMahakQueueCommand { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak queue item deleted successfully: {Id}", id);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete Mahak queue item: {Id}. Error: {Error}", id, result.ErrorMessage);
            return NotFound(result);
        }
    }
}

