using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.Product;
using OnlineShop.Application.Features.Product.Command.Create;
using OnlineShop.Application.Features.Product.Command.Delete;
using OnlineShop.Application.Features.Product.Command.Update;
using OnlineShop.Application.Features.Product.Queries.GetAll;
using OnlineShop.Application.Features.Product.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateProductCommand { Product = dto };
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateProductDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.Id = id;
            var command = new UpdateProductCommand { Product = dto };

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteProductCommand (  id ), cancellationToken);
            return result.IsSuccess ? NoContent() : NotFound(result);
        }
    }
}
