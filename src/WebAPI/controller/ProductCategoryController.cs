using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.DTOs.ProductCategory;
using OnlineShop.Application.Features.ProductCategory.Command.Create;
using OnlineShop.Application.Features.ProductCategory.Command.Delete;
using OnlineShop.Application.Features.ProductCategory.Command.Update;
using OnlineShop.Application.Features.ProductCategory.Queries.GetAll;
using OnlineShop.Application.Features.ProductCategory.Queries.GetById;

namespace OnlineShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAllProductCategoriesQuery(), cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetProductCategoryByIdQuery { Id=id }, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateProductCategoryCommand { Dto=dto };
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateProductCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            dto.Id = id;
            var command = new UpdateProductCategoryCommand {   Dto= dto };

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteProductCategoryCommand { Id = id }, cancellationToken);
            return result.IsSuccess ? NoContent() : NotFound(result);
        }
    }
}
