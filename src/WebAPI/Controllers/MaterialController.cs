using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Material;
using OnlineShop.Application.Features.Material.Commands.Create;
using OnlineShop.Application.Features.Material.Commands.Update;
using OnlineShop.Application.Features.Material.Commands.Delete;
using OnlineShop.Application.Features.Material.Queries.GetAll;
using OnlineShop.Application.Features.Material.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMediator mediator, ILogger<MaterialController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all materials");
            var result = await _mediator.Send(new GetAllMaterialsQuery(), cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting material by ID: {MaterialId}", id);
            var result = await _mediator.Send(new GetMaterialByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating material: {MaterialName}", dto.Name);
            var command = new CreateMaterialCommand { Material = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMaterialDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating material: {MaterialId}", id);
            dto.Id = id;
            var command = new UpdateMaterialCommand { Material = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting material: {MaterialId}", id);
            var result = await _mediator.Send(new DeleteMaterialCommand(id), cancellationToken);
            
            if (result.IsSuccess)
                return NoContent();
            
            return NotFound(result);
        }
    }
}
