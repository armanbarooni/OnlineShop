using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.MahakMapping;
using OnlineShop.Application.Features.MahakMapping.Command.Create;
using OnlineShop.Application.Features.MahakMapping.Command.Delete;
using OnlineShop.Application.Features.MahakMapping.Command.Update;
using OnlineShop.Application.Features.MahakMapping.Queries.GetAll;
using OnlineShop.Application.Features.MahakMapping.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MahakMappingController(IMediator mediator, ILogger<MahakMappingController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<MahakMappingController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all Mahak mappings");
            var result = await _mediator.Send(new GetAllMahakMappingsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} Mahak mappings", result.Data?.Count ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve Mahak mappings: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting Mahak mapping by ID: {Id}", id);
            var result = await _mediator.Send(new GetMahakMappingByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved Mahak mapping: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Mahak mapping not found: {Id}", id);
            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMahakMappingDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating Mahak mapping by user: {UserId}", userId);
            
            var command = new CreateMahakMappingCommand { MahakMapping = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak mapping created successfully: {Id}", result.Data?.Id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to create Mahak mapping. Error: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMahakMappingDto dto, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating Mahak mapping: {Id} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateMahakMappingCommand { MahakMapping = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak mapping updated successfully: {Id}", id);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to update Mahak mapping: {Id}. Error: {Error}", id, result.ErrorMessage);
            return NotFound(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting Mahak mapping: {Id} by user: {UserId}", id, userId);
            
            var result = await _mediator.Send(new DeleteMahakMappingCommand { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Mahak mapping deleted successfully: {Id}", id);
                return NoContent();
            }
            
            _logger.LogWarning("Failed to delete Mahak mapping: {Id}. Error: {Error}", id, result.ErrorMessage);
            return NotFound(result);
        }
    }
}

