using Microsoft.AspNetCore.Mvc;
using MediatR;

using OnlineShop.Application.Features.Unit.Queries.GetById;

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

        [HttpGet()]
        public async Task<IActionResult> GetAll([FromRoute] Guid id ,  CancellationToken cancellationToken = default)
        {
            var query = new GetUnitByIdQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

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

        ///// <summary>
        ///// ویرایش واحد
        ///// </summary>
        //[HttpPut("{id:int}")]
        //public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateUnitCommand command, CancellationToken cancellationToken = default)
        //{
        //    if (id != command.Id)
        //        return BadRequest("شناسه در مسیر و بدنه یکسان نیست");

        //    var result = await _mediator.Send(command, cancellationToken);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        ///// <summary>
        ///// حذف واحد
        ///// </summary>
        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken = default)
        //{
        //    var command = new DeleteUnitCommand { Id = id };
        //    var result = await _mediator.Send(command, cancellationToken);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}
    }
}
