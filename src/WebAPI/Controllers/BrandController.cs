using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Brand;
using OnlineShop.Application.Features.Brand.Commands.Create;
using OnlineShop.Application.Features.Brand.Commands.Update;
using OnlineShop.Application.Features.Brand.Commands.Delete;
using OnlineShop.Application.Features.Brand.Queries.GetAll;
using OnlineShop.Application.Features.Brand.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IMediator mediator, ILogger<BrandController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all brands");
            var result = await _mediator.Send(new GetAllBrandsQuery(), cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting brand by ID: {BrandId}", id);
            var result = await _mediator.Send(new GetBrandByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBrandDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating brand: {BrandName}", dto.Name);
            var command = new CreateBrandCommand { Brand = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBrandDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating brand: {BrandId}", id);
            dto.Id = id;
            var command = new UpdateBrandCommand { Brand = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting brand: {BrandId}", id);
            var result = await _mediator.Send(new DeleteBrandCommand(id), cancellationToken);
            
            if (result.IsSuccess)
                return NoContent();
            
            return NotFound(result);
        }
    }
}

