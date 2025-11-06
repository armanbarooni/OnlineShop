using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Season;
using OnlineShop.Application.Features.Season.Commands.Create;
using OnlineShop.Application.Features.Season.Commands.Update;
using OnlineShop.Application.Features.Season.Commands.Delete;
using OnlineShop.Application.Features.Season.Queries.GetAll;
using OnlineShop.Application.Features.Season.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SeasonController> _logger;

        public SeasonController(IMediator mediator, ILogger<SeasonController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all seasons");
            var result = await _mediator.Send(new GetAllSeasonsQuery(), cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting season by ID: {SeasonId}", id);
            var result = await _mediator.Send(new GetSeasonByIdQuery { Id = id }, cancellationToken);
            
            if (result.IsSuccess)
                return Ok(result);
            
            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSeasonDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating season: {SeasonName}", dto.Name);
            var command = new CreateSeasonCommand { Season = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
            
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSeasonDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating season: {SeasonId}", id);
            
            // Validate ID mismatch
            if (dto.Id != Guid.Empty && dto.Id != id)
            {
                return BadRequest(new { isSuccess = false, message = "شناسه در مسیر و بدنه درخواست مطابقت ندارد" });
            }
            
            dto.Id = id;
            var command = new UpdateSeasonCommand { Season = dto };
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok(result);
            
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting season: {SeasonId}", id);
            var result = await _mediator.Send(new DeleteSeasonCommand(id), cancellationToken);
            
            if (result.IsSuccess)
                return NoContent();
            
            return NotFound(result);
        }
    }
}
