using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Features.Unit.Command.Create;
using OnlineShop.Application.Features.Unit.Command.Delete;
using OnlineShop.Application.Features.Unit.Command.Update;
using OnlineShop.Application.Features.Unit.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetUnitByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
                return Ok(result);

            return NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            // ✅ CreateUnitCommand از property استفاده می‌کنه
            var command = new CreateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                // ✅ result.Data خودش Guid هست (طبق CreateUnitCommandHandler)
                return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitDto dto)
        {
            // ✅ Id رو به dto اضافه می‌کنیم
            dto.Id = id;
            var command = new UpdateUnitCommand { UnitDto = dto };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // ⚠️ فایل DeleteUnitCommand.cs رو ندیدم - احتمالاً همین شکلیه:
            var command = new DeleteUnitCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return NoContent();

            return NotFound(result);
        }
    }
}
