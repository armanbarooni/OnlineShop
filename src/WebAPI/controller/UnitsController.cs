using Microsoft.AspNetCore.Mvc;
using MediatR;

using OnlineShop.Application.Features.Unit.Queries.GetById;
using OnlineShop.Application.Features.Unit.Command.Create;
using OnlineShop.Application.DTOs.Unit;
using OnlineShop.Application.Features.Unit.Command.Update;
using OnlineShop.Application.Features.Unit.Command.Delete;

namespace OnlineShop.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[HttpGet()]
        //public async Task<IActionResult> GetAll([FromRoute] Guid id ,  CancellationToken cancellationToken = default)
        //{
        //    var query = new GetUnitByIdQuery();
        //    var result = await _mediator.Send(query, cancellationToken);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        [HttpGet("{id}/UnitDeatils")]
        public async Task<IActionResult> GetOneById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var query = new GetUnitByIdQuery
            {
                Id = id
            };
            var result = await _mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUnitCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpDelete("{id}/DeleteUnit")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var command = new DeleteUnitCommand { Id = id };
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
