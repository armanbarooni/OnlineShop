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
    public class UnitController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllUnitsQuery(), cancellationToken);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        private readonly IMediator _mediator;

        public UnitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetUnitByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
                return Ok(result);

            return NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            var command = new CreateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
        {
            dto.Id = id;
            var command = new UpdateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteUnitCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return NoContent();

            return NotFound(result);
        }
    }
}


