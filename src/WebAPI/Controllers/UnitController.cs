using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Features.Unit.Command.Create;
using OnlineShop.Application.Features.Unit.Command.Delete;
using OnlineShop.Application.Features.Unit.Command.Update;
using OnlineShop.Application.Features.Unit.Queries.GetById;
using OnlineShop.Application.Features.Unit.Queries.GetAll;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController(IMediator mediator, ILogger<UnitController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<UnitController> _logger = logger;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all units");
            var result = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} units", result.Data?.Count() ?? 0);
                return Ok(result);
            }
            
            _logger.LogWarning("Failed to retrieve units: {Error}", result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Getting unit by ID: {UnitId}", id);
            var query = new GetUnitByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved unit: {UnitId}", id);
                return Ok(result);
            }

            _logger.LogWarning("Unit not found: {UnitId}", id);
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Creating unit: {UnitName} by user: {UserId}", dto.Name, userId);
            
            var command = new CreateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Unit created successfully: {UnitId} by user: {UserId}", result.Data, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
            }

            _logger.LogWarning("Failed to create unit: {UnitName} by user: {UserId}. Error: {Error}", 
                dto.Name, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Updating unit: {UnitId} by user: {UserId}", id, userId);
            
            dto.Id = id;
            var command = new UpdateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Unit updated successfully: {UnitId} by user: {UserId}", id, userId);
                return Ok(result);
            }

            _logger.LogWarning("Failed to update unit: {UnitId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Deleting unit: {UnitId} by user: {UserId}", id, userId);
            
            var command = new DeleteUnitCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Unit deleted successfully: {UnitId} by user: {UserId}", id, userId);
                return NoContent();
            }

            _logger.LogWarning("Failed to delete unit: {UnitId} by user: {UserId}. Error: {Error}", 
                id, userId, result.ErrorMessage);
            return NotFound(result);
        }
    }
}